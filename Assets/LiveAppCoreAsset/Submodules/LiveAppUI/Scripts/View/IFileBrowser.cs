using Cysharp.Threading.Tasks;

namespace LiveAppCore.Domain
{
    public interface IFileBrowser
    {
        UniTask<string> GetSavePath( string title, string defaultPath, string[] extensions );
        UniTask<string> GetFolderPath( string title, string defaultPath );
        UniTask<string> GetLoadPath( string title, string defaultPath, string[] extensions );
        UniTask<string[]> GetLoadPathList( string title, string defaultPath, string[] extensions );
    }
}
