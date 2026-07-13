using Cysharp.Threading.Tasks;
using LiveAppCore.Editor.Application;
using LiveAppCore.Editor.Domain;
using System;
using UnityEditor;
using UnityEngine;

namespace LiveAppCore.Editor.View
{
    public static class AssetBundleBuildView
    {
        public static async UniTask<bool> ExecuteAssetBundleBuild( string platform )
        {
            var isSuccessParse = Enum.TryParse<BuildTarget>(platform, out var target);
            if(!isSuccessParse || !Enum.IsDefined( typeof( BuildTarget ), target ))
            {
                throw new Exception( $"Invalid Platform :: {target}" );
            }

            var targetGroup = default(BuildTargetGroup);
            IAssetBundleBuildDomain domain = null;
            switch(target)
            {

                case BuildTarget.StandaloneWindows64:
                case BuildTarget.StandaloneWindows:
                    targetGroup = BuildTargetGroup.Standalone;
                    domain = new StanaloneAssetBundleBuilder();
                    break;

                case BuildTarget.Android:
                    targetGroup = BuildTargetGroup.Android;
                    break;

                case BuildTarget.iOS:
                    targetGroup = BuildTargetGroup.iOS;
                    domain = new IOSAssetBundleBuilder();
                    break;

                default:
                    throw new Exception( $"Invalid Platform :: {target}" );
            }

            var application = new AssetBundleBuildApplication(domain);
            var result = await application.ExecuteAssetBundleBuild(targetGroup);
            if(result == false)
            {
                throw new Exception( "AssetBundle Build Failed" );
            }
            Debug.Log( "AssetBundle Build Success" );
            return true;
        }
    }
}
