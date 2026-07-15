using Cysharp.Threading.Tasks;

namespace StudioSystemSDK.Application
{
    /// <summary>
    /// 파일 취득 프로세스 관련 Application
    /// </summary>
    public interface IFileSystemContext
    {
        /// <summary>
        /// Binary 파일 취득
        /// </summary>
        /// <param name="path">파일 경로/파일명</param>
        /// <returns>binary파일의 문자열 데이터</returns>
        UniTask<string> ReadBinaryFile( string path );
    }
}
