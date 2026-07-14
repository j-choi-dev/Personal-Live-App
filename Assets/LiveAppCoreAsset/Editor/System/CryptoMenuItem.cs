using Cysharp.Threading.Tasks;
using LiveAppCore.Editor.View;
using UnityEditor;
using UnityEngine;

namespace LiveAppCore.Editor
{
    /// <summary>
    /// 암호화/복호화 관련 Unity Custom Menu
    /// </summary>
    public class CryptoMenuItem
    {
        private const string MENU_NAME_ENCRYPTION = "LiveAppTool/File Crypto/Encryption";
        [MenuItem( MENU_NAME_ENCRYPTION, priority = 20 )]
        private static async UniTask<bool> FileEncrypt()
        {
            var platform = EditorUserBuildSettings.activeBuildTarget.ToString();
            return await RomBuildView.ExecuteRomBuild( platform );
        }
    }
}
