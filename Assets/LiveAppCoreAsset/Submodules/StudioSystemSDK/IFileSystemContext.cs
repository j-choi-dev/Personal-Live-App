using Cysharp.Threading.Tasks;

namespace StudioSystemSDK.Application
{
    public interface IFileSystemContext
    {
        UniTask<string> ReadBinaryFile( string path );
    }
}
