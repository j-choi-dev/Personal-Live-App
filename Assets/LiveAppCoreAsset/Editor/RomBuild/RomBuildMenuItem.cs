using Cysharp.Threading.Tasks;
using LiveAppCore.Editor.View;
using UnityEditor;

namespace LiveAppCore.Editor
{
    public class RomBuildMenuItem
    {
        private const string MENU_NAME_iOS_BUILD_ONLY = "LiveAppTool/ROM(iOS) Build";
        [MenuItem( MENU_NAME_iOS_BUILD_ONLY, priority = 10 )]
        private static async UniTask<bool> iosRomBuild()
        {
            var platform = EditorUserBuildSettings.activeBuildTarget.ToString();
            return await RomBuildView.ExecuteRomBuild( platform );
        }
    }
}
