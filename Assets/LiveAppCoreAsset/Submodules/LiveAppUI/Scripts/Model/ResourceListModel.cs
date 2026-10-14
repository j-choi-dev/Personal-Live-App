using Cysharp.Threading.Tasks;
using StudioNetworkSDK.Domain;
using StudioResourceSDK.Application;
using StudioResourceSDK.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace LiveAppUI.Model
{
    /// <summary>
    /// Resource List Model 구현체
    /// </summary>
    public class ResourceListModel : IResourceListModel, IDisposable
    {
        private IResourceServerConfigContext _resourceConfigContext;
        private IResourceTableContext _resourceTableContext;
        private IResourceLoadContext _resourceLoadContext;
        private bool _configurationReady;
        private bool _isLoading;

        private CompositeDisposable _disposable = new CompositeDisposable();

        private Dictionary<ResourceType, ServerType> _serverTypeDic = new Dictionary<ResourceType, ServerType>();
        private IList<ResourceServerData> _serverConfigs = new List<ResourceServerData>();

        private Subject<IReadOnlyList<(string id, string displayName)>> _onCharacterListChanged = new Subject<IReadOnlyList<(string id, string displayName)>>();
        public IObservable<IReadOnlyList<(string id, string displayName)>> OnCharacterListChanged => _onCharacterListChanged;

        private readonly Subject<ResourceListFailureData> _onLoadFailed = new Subject<ResourceListFailureData>();

        public IObservable<ResourceListFailureData> OnLoadFailed => _onLoadFailed;
        public bool IsLoading => _isLoading;

        public ResourceListModel( IResourceServerConfigContext resourceConfigContext,
            IResourceTableContext resourceTableContext,
            IResourceLoadContext resourceLoadContext )
        {
            _resourceConfigContext = resourceConfigContext;
            _resourceTableContext = resourceTableContext;
            _resourceLoadContext = resourceLoadContext;

            _resourceTableContext.OnCharacterListChanged
                .Subscribe( list =>
                {
                    var result = list.Select( arg => (arg.ID, arg.DisplayName) )
                        .ToList();
                    _onCharacterListChanged.OnNext( result );
                } )
                .AddTo( _disposable );

            // TODO Refactor 대상(별도 유틸 스크립트화) @Choi 26.07.06
            var resourceTypeList = Enum.GetValues(typeof(ResourceType))
                .Cast<ResourceType>()
                .ToList();
            for( int i = 0; i < resourceTypeList.Count; i++ )
            {
                if( resourceTypeList[i] ==  ResourceType.None )
                {
                    continue;
                }
                _serverTypeDic.Add( resourceTypeList[i], ServerType.None );
            }
        }

        public async UniTask<bool> InitializeServerConfig()
        {
            _configurationReady = false;
            try
            {
                var cloudConfigTask = await _resourceConfigContext.LoadCloudConfig();
                if (cloudConfigTask == null)
                {
                    UnityEngine.Debug.LogError("Cloud 설정을 불러오지 못했습니다.");
                    return false;
                }
                var serverConfigTask = await _resourceConfigContext.LoadServerConfig();
                if (serverConfigTask == null)
                {
                    UnityEngine.Debug.LogError("Resource 서버 설정을 불러오지 못했습니다.");
                    return false;
                }
                _serverConfigs = serverConfigTask.ToList();
                _configurationReady = true;
                return true;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogException(ex);
                return false;
            }
            // 리소스 & 구글 시트 링크 보존 -> Context 통해서 DataClass로 ...? @Choi
        }

        public async UniTask GetResourceList( ResourceType resourceType, ServerType serverType )
        {
            await GetResourceListCoreAsync( resourceType, serverType, reauthorize: false);
        }
        private async UniTask GetResourceListCoreAsync(
    ResourceType resourceType,
    ServerType serverType,
    bool reauthorize)
        {
            if (_isLoading)
                return;

            _isLoading = true;

            try
            {
                if (!_configurationReady)
                {
                    var initialized = await InitializeServerConfig();

                    if (!initialized)
                    {
                        PublishFailure(
                            ResourceListFailureKind.ConfigurationInitializationFailed,
                            resourceType,
                            serverType);
                        return;
                    }
                }

                var resource = ConvertResourceType(resourceType);
                var server = ConvertServerType(serverType);

                var config = _serverConfigs.FirstOrDefault(
                    arg => arg.resourceType == resource &&
                           arg.serverType == server);

                if (config == null)
                {
                    PublishFailure(
                        ResourceListFailureKind.MissingConfiguration,
                        resourceType,
                        serverType);
                    return;
                }

                var loaded = reauthorize
                    ? await _resourceTableContext.ReauthorizeAndLoadResourceTableProcess(
                        config.resourceType,
                        server.ToString(),
                        config.tableUrl)
                    : await _resourceTableContext.LoadResourceTableProcess(
                        config.resourceType,
                        server.ToString(),
                        config.tableUrl);

                if (!loaded)
                {
                    PublishFailure(
                        ResourceListFailureKind.SheetOrTabNotFound,
                        resourceType,
                        serverType);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (ResourceTableLoadException e)
            {
                ResourceListFailureKind kind;

                switch (e.Kind)
                {
                    case ResourceTableFailureKind.Unauthorized:
                        kind = ResourceListFailureKind.Unauthorized;
                        break;

                    case ResourceTableFailureKind.PermissionDenied:
                        kind = ResourceListFailureKind.PermissionDenied;
                        break;

                    case ResourceTableFailureKind.Network:
                        kind = ResourceListFailureKind.Network;
                        break;

                    case ResourceTableFailureKind.AuthorizationCancelled:
                        kind = ResourceListFailureKind.AuthorizationCancelled;
                        break;

                    default:
                        kind = ResourceListFailureKind.Other;
                        break;
                }

                PublishFailure(
                    kind,
                    resourceType,
                    serverType,
                    e.StatusCode);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);

                PublishFailure(
                    ResourceListFailureKind.Other,
                    resourceType,
                    serverType);
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void PublishFailure( ResourceListFailureKind kind, ResourceType resourceType, ServerType serverType, long statusCode = 0)
        {
            _onLoadFailed.OnNext( new ResourceListFailureData( kind, resourceType, serverType, statusCode ));
        }

        public async UniTask ReauthorizeAndGetResourceList( ResourceType resourceType, ServerType serverType )
        {
            await GetResourceListCoreAsync( resourceType, serverType, reauthorize: true );
        }

        public ServerType GetCurrentServerType( ResourceType resourceType )
            => _serverTypeDic[resourceType];

        public void SetCurrentServerType( ResourceType resourceType, ServerType serverType )
            => _serverTypeDic[resourceType] = serverType;

        public async UniTask<bool> LoadResourceProcess( ResourceType resourceType,
            ServerType serverType,
            IReadOnlyList<string> resourceId )
        {
            return await _resourceLoadContext.LoadResource( 
                ConvertResourceType( resourceType ), 
                serverType, 
                resourceId );
        }

        private StudioResourceSDK.Domain.ResourceType ConvertResourceType( ResourceType type )
            => ( StudioResourceSDK.Domain.ResourceType )Enum.Parse( 
                typeof( StudioResourceSDK.Domain.ResourceType ), 
                type.ToString() );

        private StudioNetworkSDK.Domain.ServerType ConvertServerType( ServerType type )
            => ( StudioNetworkSDK.Domain.ServerType )Enum.Parse( 
                typeof( StudioNetworkSDK.Domain.ServerType ), 
                type.ToString() );

        public void Dispose()
        {
            _disposable.Dispose();
            _disposable = null;

            _onCharacterListChanged.Dispose();
            _onLoadFailed.Dispose();
        }
    }
}
