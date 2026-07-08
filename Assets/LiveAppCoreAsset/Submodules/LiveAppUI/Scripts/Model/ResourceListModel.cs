using Cysharp.Threading.Tasks;
using StudioNetworkSDK.Domain;
using StudioResourceSDK.Application;
using StudioSystemSDK.Application;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UniRx;

namespace LiveAppUI.Model
{
    public class ResourceListModel : IResourceListModel, IDisposable
    {
        private const string TempFileName = "ResourceInfo.bin"; // TODO 리팩터링 대상 @Choi 26.07.04
        private IResourceConfigContext _resourceConfigContext;
        private IResourceTableContext _resourceTableContext;
        private IFileSystemContext _fileSystemContext;
        private CompositeDisposable _disposable = new CompositeDisposable();

        private Dictionary<ResourceType, ServerType> _serverTypeDic = new Dictionary<ResourceType, ServerType>();
        private IList<ResourceServerData> _serverConfigs = new List<ResourceServerData>();

        private Subject<IReadOnlyList<(string id, string displayName)>> _onCharacterListChanged = new Subject<IReadOnlyList<(string id, string displayName)>>();
        public IObservable<IReadOnlyList<(string id, string displayName)>> OnCharacterListChanged => _onCharacterListChanged;

        public ResourceListModel( IResourceConfigContext resourceConfigContext,
            IResourceTableContext resourceTableContext,
            IFileSystemContext fileSystemContext )
        {
            _resourceConfigContext = resourceConfigContext;
            _resourceTableContext = resourceTableContext;
            _fileSystemContext = fileSystemContext;

            _resourceTableContext.OnCharacterListChanged
                .Subscribe(list =>
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

        public async UniTask InitializeServerConfig()
        {
            var rawData = await _fileSystemContext.ReadBinaryFile( TempFileName );
            _serverConfigs =  _resourceConfigContext.ParseServerConfigData(rawData).ToList();
            // 리소스 & 구글 시트 링크 보존 -> Context 통해서 DataClass로 ...? @Choi
        }

        public async UniTask GetResourceList( ResourceType resourceType, ServerType serverType )
        {
            if( _serverTypeDic[resourceType] == serverType )
            {
                return;
            }
            var resource = ConvertResourceType(resourceType);
            var server = ConvertServerType(serverType);
            var config = _serverConfigs
                .FirstOrDefault(arg => arg.resourceType == resource &&
                arg.serverType == server);

            var loadResult = await _resourceTableContext.LoadResourceTableProcess( 
                config.resourceType, 
                server.ToString(), 
                config.tableUrl 
                );
        }

        public ServerType GetCurrentServerType( ResourceType resourceType )
            => _serverTypeDic[resourceType];

        public void SetCurrentServerType( ResourceType resourceType, ServerType serverType )
            => _serverTypeDic[resourceType] = serverType;


        private StudioResourceSDK.Domain.ResourceType ConvertResourceType( ResourceType type )
        {
            StudioResourceSDK.Domain.ResourceType retVal
                = (StudioResourceSDK.Domain.ResourceType)Enum.Parse(
                    typeof(StudioResourceSDK.Domain.ResourceType), type.ToString()
                    );
            return retVal;
        }

        private StudioNetworkSDK.Domain.ServerType ConvertServerType(ServerType type)
        {
            StudioNetworkSDK.Domain.ServerType retVal 
                = (StudioNetworkSDK.Domain.ServerType)Enum.Parse(
                    typeof(StudioNetworkSDK.Domain.ServerType), type.ToString()
                    );
            return retVal;
        }

        public void Dispose()
        {
            _disposable.Dispose();
            _disposable = null;
        }
    }
}
