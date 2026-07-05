using Cysharp.Threading.Tasks;
using StudioNetworkSDK.Domain;
using StudioResourceSDK.Application;
using StudioSystemSDK.Application;
using StudioSystemSDK.Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace LiveAppUI.Model
{
    public class ResourceListModel : IResourceListModel
    {
        private const string TempFileName = "ResourceInfo.bin"; // TODO 리팩터링 대상 @Choi 26.07.04
        private IResourceConfigContext _resourceTableInfoContext;
        private IResourceTableContext _resourceTableContext;
        private IFileSystemContext _fileSystemContext;
        ReadOnlyCollection <ResourceServerData> _configCollection;

        public ResourceListModel( IResourceConfigContext resourceTableInfoContext,
            IResourceTableContext resourceTableContext,
            IFileSystemContext fileSystemContext )
        {
            _resourceTableInfoContext = resourceTableInfoContext;
            _resourceTableContext = resourceTableContext;
            _fileSystemContext = fileSystemContext;
        }

        public async UniTask InitializeServerConfig()
        {
            var rawData = await _fileSystemContext.ReadBinaryFile( TempFileName );
            var serverConfig =  _resourceTableInfoContext.ParseServerConfigData(rawData);

        }

        public async UniTask<IReadOnlyList<string>> GetCharacterList()
        {
            throw new NotImplementedException();
        }
    }
}
