namespace StudioSystemSDK.Domain
{
    public interface IFileSystemDomain
    {
        bool IsDirectoryExist( string dirName );
        bool IsFileExist( string fileName );
        void CreateDirectory(string dirName );
        bool CreateFile( string dirName );
        bool SaveBinaryFile( string filePath, byte[] message );
        byte[] LoadBinaryFile( string filePath );
    }
}
