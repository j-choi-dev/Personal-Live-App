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
            var rawData = string.Empty;
            try
            {

                if( _fileSystemDomain.IsFileExist( path ) == false )
                {
                    throw new FileNotFoundException( "File Not Exist :: ", path );
                }
                rawData = await _fileSystemDomain.LoadTextFile( path );
                return rawData;
            }
            catch( Exception e )
            {
                UnityEngine.Debug.LogException( e );
                return string.Empty;
            }
        }

        public async UniTask<bool> SaveBinaryFile( string path, string message )
        {
            try
            {
                if( _fileSystemDomain.IsFileExist( path ) == false )
                {
                    _fileSystemDomain.CreateFile( path );
                }
                return await _fileSystemDomain.SaveTextFile( path, message );
            }
            catch( Exception e )
            {
                UnityEngine.Debug.LogException( e );
                return false;
            }
        }
    }
}
