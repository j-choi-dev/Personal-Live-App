using Cysharp.Threading.Tasks;
using StudioNetworkSDK.Domain;
using StudioResourceSDK.Domain;
using StudioSystemSDK.Domain;
using System.Collections.Generic;
using System.IO;

namespace StudioResourceSDK.Application
{
    public class ResourceConfigContext : IResourceServerConfigContext
    {
        private IResourceConfigParseDomain _domain;
        private IFileSystemDomain _fileSystemDomain;

        public ResourceConfigContext( IResourceConfigParseDomain domain,
            IFileSystemDomain fileSystemDomain )
        {
            _domain=domain;
            _fileSystemDomain = fileSystemDomain;
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
