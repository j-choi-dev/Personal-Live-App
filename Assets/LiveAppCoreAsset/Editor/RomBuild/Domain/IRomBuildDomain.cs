using Cysharp.Threading.Tasks;
using UnityEditor;

namespace LiveAppCore.Editor.Domain
{
    /// <summary>
    /// OS 및 플랫폼별 ROM Build Interface 
    /// </summary>
    public interface IRomBuildDomain
    {
        /// <summary>
        /// 본 빌드 전 처리해야 할 프로세스
        /// </summary>
        /// <param name="platform">Build Target Group</param>
        /// <returns>성공/실패</returns>
        UniTask<bool> PreProcess( BuildTargetGroup platform );
        /// <summary>
        /// 본 빌드 프로세스
        /// </summary>
        /// <param name="platform">Build Target Group</param>
        /// <returns>성공/실패</returns>
        UniTask<bool> BuildProcess( BuildTargetGroup platform );
        /// <summary>
        /// 빌드 후처리 프로세스
        /// </summary>
        /// <param name="platform">Build Target Group</param>
        /// <returns>성공/실패</returns>
        UniTask<bool> PostProcess( BuildTargetGroup platform );
    }
}
