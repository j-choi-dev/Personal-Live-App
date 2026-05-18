using Microsoft.Cci;
using StudioSystemSDK.Domain;
using System.IO;
using System.Text;
using UnityEngine;

namespace StudioSystemSDK.Infrastructure
{
    public class FileSystemInfrastructure : IFileSystemDomain
    {
        private readonly string ROOT_PATH = string.Empty;
        
        public FileSystemInfrastructure()
        {
            ROOT_PATH = UnityEngine.Application.streamingAssetsPath;
        }
        
        public bool IsDirectoryExist( string dirName )
        {
            var path = Path.Combine(ROOT_PATH, dirName);
            return Directory.Exists( path );
        }

        public void CreateDirectory( string dirName )
        {
            var path = Path.Combine(ROOT_PATH, dirName);
            if( Directory.Exists( path ) )
            {
                return;
            }
            Directory.CreateDirectory(path);
        }

        public bool IsFileExist( string filePath )
        {
            return File.Exists( filePath );
        }

        public byte[] LoadBinaryFile( string filePath )
        {
            var message = string.Empty;
            using( var fs = new FileStream( filePath, FileMode.OpenOrCreate ) )
            using( var sr = new StreamReader( filePath, false ) )
            {
                message = sr.ReadToEnd();
            }
            var bytes = Encoding.ASCII.GetBytes(message);
            return bytes;
        }

        public bool SaveBinaryFile( string filePath, byte[] message )
        {
            using( var fs = new FileStream( filePath, FileMode.OpenOrCreate ) )
            using( var sw = new StreamWriter( fs, System.Text.Encoding.UTF8 ) )
            {
                try
                {
                    sw.WriteLine( message );
                }
                catch( System.Exception e )
                {
                    Debug.LogError( e.Message );
                    return false;
                }
            }
            return true;
        }

        public bool CreateFile( string dirName )
        {
            throw new System.NotImplementedException();
        }
    }
}
