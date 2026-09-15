using Cysharp.Threading.Tasks;
using LiveAppCore.Editor.View;
using UnityEditor;
using static UnityEngine.GraphicsBuffer;

namespace LiveAppCore.Editor
{
    public class AssetBundleBuildMenuItem
    {
        private const string MENU_NAME_iOS_ASSETBUNDLE_ONLY = "LiveAppTool/AssetBundle/AssetBundle(iOS) Build";
        private const string MENU_NAME_WIN_EDITOR_ASSETBUNDLE_ONLY = "LiveAppTool/AssetBundle/AssetBundle(Win) Build";
        private const string MENU_NAME_MAC_EDITOR_ASSETBUNDLE_ONLY = "LiveAppTool/AssetBundle/AssetBundle(Mac) Build";
        private const string MENU_NAME_ALL_PLATFORM_ASSETBUNDLE_ONLY = "LiveAppTool/AssetBundle/AssetBundle(All Platform) Build";

        [MenuItem( MENU_NAME_iOS_ASSETBUNDLE_ONLY, priority = 1 )]
        private static async UniTask<bool> iOSAssetBundleBuildOnly()
        {
            var platform = EditorUserBuildSettings.activeBuildTarget.ToString();
            return await AssetBundleBuildView.ExecuteAssetBundleBuild( BuildTarget.iOS );
        }

        [MenuItem( MENU_NAME_WIN_EDITOR_ASSETBUNDLE_ONLY, priority = 0 )]
        private static async UniTask<bool> WindowsAssetBundleBuildOnly()
        {
            var platform = EditorUserBuildSettings.activeBuildTarget.ToString();
            return await AssetBundleBuildView.ExecuteAssetBundleBuild( BuildTarget.StandaloneWindows64 );
        }

        [MenuItem( MENU_NAME_MAC_EDITOR_ASSETBUNDLE_ONLY, priority = 2 )]
        private static async UniTask<bool> MacAssetBundleBuildOnly()
        {
            var platform = EditorUserBuildSettings.activeBuildTarget.ToString();
            return await AssetBundleBuildView.ExecuteAssetBundleBuild( BuildTarget.StandaloneOSX );
        }

        [MenuItem( MENU_NAME_ALL_PLATFORM_ASSETBUNDLE_ONLY, priority = 10 )]
        private static async UniTask<bool> AllPlatformAssetBundleBuildOnly()
        {
            var platform = EditorUserBuildSettings.activeBuildTarget.ToString();
            var platforms = new BuildTarget[] { BuildTarget.iOS, BuildTarget.StandaloneWindows64, BuildTarget.StandaloneOSX };
            for(var i = 0; i < platforms.Length; i++ )
            {
                await AssetBundleBuildView.ExecuteAssetBundleBuild( platforms[i] );
            }
            return true;
        }
    }
}
