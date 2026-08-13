using Cysharp.Threading.Tasks;
using UnityEditor;

namespace LiveAppCore.Editor.Domain
{
    public interface IAssetBundleBuildDomain
    {
        UniTask<bool> PreProcess( BuildTargetGroup platform );
        UniTask<bool> BuildProcess( BuildTargetGroup platform );
        UniTask<bool> PostProcess( BuildTargetGroup platform );
    }
}
