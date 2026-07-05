using Cysharp.Threading.Tasks;
using StudioSystemSDK.Domain;
using System;
using System.IO;

namespace StudioSystemSDK.Application
{
    public class FileSystemContext : IFileSystemContext
    {
        private IFileSystemDomain _fileSystemDomain;
        public FileSystemContext( IFileSystemDomain fileSystemDomain )
        {
            _fileSystemDomain = fileSystemDomain;
        }

        public async UniTask<string> ReadBinaryFile( string path )
        {
            var targetPath = Path.Combine(UnityEngine.Application.streamingAssetsPath, path);
            var rawData = string.Empty;
            try
            {

                if( _fileSystemDomain.IsFileExist( targetPath ) == false )
                {
                    throw new FileNotFoundException( "File Not Exist :: ", targetPath );
                }
                rawData = await _fileSystemDomain.LoadTextFile( targetPath );
            }
            catch( Exception e )
            {
                UnityEngine.Debug.LogException( e );
                return string.Empty;
            }
            return rawData;
        }
    }
}
