using Cysharp.Threading.Tasks;
using StudioNetworkSDK.Domain;
using StudioResourceSDK.Application;
using StudioSystemSDK.Application;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UniRx;

namespace LiveAppUI.Model
{
    public class ResourceListModel : IResourceListModel
    {
        private const string TempFileName = "ResourceInfo.bin"; // TODO 리팩터링 대상 @Choi 26.07.04
        private IResourceConfigContext _resourceConfigContext;
        private IResourceTableContext _resourceTableContext;
        private IFileSystemContext _fileSystemContext;

        private ResourceType _currentResourceType = ResourceType.None;
        private Dictionary<ResourceType, ServerType> _serverTypeDic = new Dictionary<ResourceType, ServerType>();

        private Subject<IReadOnlyList<string>> _onCharacterListChanged = new Subject<IReadOnlyList<string>>();
        public IObservable<IReadOnlyList<string>> OnCharacterListChanged => _onCharacterListChanged;

        public ResourceListModel( IResourceConfigContext resourceConfigContext,
            IResourceTableContext resourceTableContext,
            IFileSystemContext fileSystemContext )
        {
            _resourceConfigContext = resourceConfigContext;
            _resourceTableContext = resourceTableContext;
            _fileSystemContext = fileSystemContext;

            // TODO Refactor 대상 @Choi 26.07.06
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
            _currentResourceType = ResourceType.Character;
        }

        public async UniTask InitializeServerConfig()
        {
            var rawData = await _fileSystemContext.ReadBinaryFile( TempFileName );
            var serverConfigs =  _resourceConfigContext.ParseServerConfigData(rawData);
            var tempList = serverConfigs.Select( arg => arg ).ToList();
            // 리소스 & 구글 시트 링크 보존 -> Context 통해서 DataClass로 ...? @Choi
        }

        public async UniTask GetResourceList( ResourceType resourceType, ServerType serverType )
        {
            if( _serverTypeDic[resourceType] == serverType )
            {
                return;
            }
            _serverTypeDic[resourceType] = serverType;

            throw new NotImplementedException();
        }

        public ServerType GetCurrentServerType( ResourceType resourceType )
            => _serverTypeDic[resourceType];
    }
}
