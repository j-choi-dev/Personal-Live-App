#if UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

namespace MqttEcho.EditorTools
{
    /// <summary>
    /// iOS 빌드가 끝난 뒤 Xcode 프로젝트의 Info.plist에 로컬 네트워크 권한 문구를 넣는다.
    /// iOS 14부터 같은 Wi-Fi의 다른 기기(서버 PC)에 접속하려면 이 문구가 있어야 권한 창이 뜬다.
    /// Xcode에서 직접 넣어도 되지만, 빌드할 때마다 지워지므로 자동으로 넣는다.
    /// (Editor 폴더 안에 있어야 앱 빌드에 포함되지 않음)
    /// </summary>
    public static class IosLocalNetworkPostProcess
    {
        const string Key = "NSLocalNetworkUsageDescription";
        const string Message = "같은 네트워크에 있는 테스트 서버에 접속하기 위해 필요합니다.";

        [PostProcessBuild]
        public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (target != BuildTarget.iOS) return;

            var plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
            var plist = new PlistDocument();
            plist.ReadFromFile(plistPath);
            plist.root.SetString(Key, Message);
            plist.WriteToFile(plistPath);
        }
    }
}
#endif
