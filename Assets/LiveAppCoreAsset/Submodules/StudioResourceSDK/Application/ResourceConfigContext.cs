using Cysharp.Threading.Tasks;
using StudioNetworkSDK.Domain;
using StudioResourceSDK.Domain;
using StudioSystemSDK.Domain;
using StudioSystemSDK.Infrastructure;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace StudioResourceSDK.Application
{
    public class ResourceConfigContext : IResourceServerConfigContext
    {
        private IResourceConfigParseDomain _domain;
        private IFileSystemDomain _fileSystemDomain;
        private IResourceDownloadDomain _resourceLoadDomain;
        private ICloudConfigParseDomain _cloudConfigParseDomain;
        private ICryptoProcessDomain _cryptoProcessDomain;
        private ICryptoKeySettingDomain _cryptoKeySettingDomain;

        public ResourceConfigContext( IResourceConfigParseDomain domain,
            IFileSystemDomain fileSystemDomain,
            IResourceDownloadDomain downloadDomain,
            ICloudConfigParseDomain cloudConfigParseDomain,
            ICryptoProcessDomain cryptoProcessDomain,
            ICryptoKeySettingDomain cryptoKeySettingDomain )
        {
            _domain=domain;
            _fileSystemDomain = fileSystemDomain;
            _resourceLoadDomain = downloadDomain;
            _cloudConfigParseDomain = cloudConfigParseDomain;
            _cryptoProcessDomain = cryptoProcessDomain;
            _cryptoKeySettingDomain = cryptoKeySettingDomain;
        }

        public async UniTask<bool> LoadCloudConfig()
        {
            var originPath = Path.Combine(SystemPathValue.ConfigOriginRoot, ResourceConstValue.ReosurceCloudConfigFille);
            var destPath = Path.Combine(SystemPathValue.ConfigDestinationRoot, ResourceConstValue.ReosurceCloudConfigFille);
            var isOriginExists = _fileSystemDomain.IsFileExist(originPath);
            if( isOriginExists == false )
            {
                throw new FileNotFoundException( "Original Path File does not exist.", originPath );
            }
            var isDestExists = _fileSystemDomain.IsFileExist(destPath);
            var isEqulaFile = await _fileSystemDomain.IsEqual( originPath, destPath );
            if( isDestExists == false || isEqulaFile == false )
            {
                _fileSystemDomain.CopyFile( originPath, destPath, true );
            }
            var rawData = await _fileSystemDomain.LoadTextFile(destPath);
            var decryptedText = _cryptoProcessDomain.ConvertDecryptedString( rawData, _cryptoKeySettingDomain.CryptoKey );
            var config = _cloudConfigParseDomain.ParseData(decryptedText);
            return await _resourceLoadDomain.InitProcess( config );
        }

        public async UniTask<IReadOnlyCollection<ResourceServerData>> LoadServerConfig()
        {
            try
            {
                var originPath = Path.Combine(SystemPathValue.ConfigOriginRoot, ResourceConstValue.BinFileName);
                var destPath = Path.Combine(SystemPathValue.ConfigDestinationRoot, ResourceConstValue.BinFileName);

                var isOriginExists = _fileSystemDomain.IsFileExist(originPath);
                if( isOriginExists == false )
                {
                    throw new FileNotFoundException( "Original Path File does not exist.", originPath );
                }
                var isDestExists = _fileSystemDomain.IsFileExist(destPath);
                var isEqulaFile = await _fileSystemDomain.IsEqual( originPath, destPath );
                if( isDestExists == false || isEqulaFile == false)
                {
                    _fileSystemDomain.CopyFile( originPath, destPath, true );
                }
                var rawData = await _fileSystemDomain.LoadTextFile(destPath);
                return _domain.ParseServerConfigData( rawData );
            }
            catch(System.Exception e)
            {
                throw new System.Exception( e.Message );
            }
        }
    }
}
