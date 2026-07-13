using Cysharp.Threading.Tasks;
using LiveAppCore.Editor.View;
using UnityEditor;

namespace LiveAppCore.Editor
{
    public class AssetBundleBuildMenuItem
    {
        private const string MENU_NAME_ASSETBUNDLE_ONLY = "LiveAppTool/AssetBundleBuild";
        [MenuItem( MENU_NAME_ASSETBUNDLE_ONLY, priority = 0 )]
        private static async UniTask<bool> AssetBundleBuildOnly()
        {
            var platform = EditorUserBuildSettings.activeBuildTarget.ToString();
            return await AssetBundleBuildView.ExecuteAssetBundleBuild( platform );
        }
    }
}
