using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
//#if UNITY_EDITOR
using UnityEditor;
using LiveAppCore.Editor.Application;
using LiveAppCore.Editor.Domain;
using LiveAppCore.Editor.Infrastructure;

namespace LiveAppCore.Editor.View
{
    public static class RomBuildView
    {
        /// <summary>
        /// Rom Build 실행.
        /// </summary>
        /// <param name="platform"></param>
        /// <returns></returns>
        public static async UniTask<bool> ExecuteRomBuild( string platform )
        {
            try
            {
                var isSuccessParse = Enum.TryParse<BuildTarget>(platform, out var target);
                if( !isSuccessParse || Enum.IsDefined( typeof( BuildTarget ), target ) == false )
                {
                    throw new Exception( $"Invalid Platform :: {target}" );
                }

                var targetGroup = default(BuildTargetGroup);
                IRomBuildDomain domain = null;
                switch( target )
                {
                    case BuildTarget.StandaloneWindows64:
                    case BuildTarget.StandaloneWindows:
                    case BuildTarget.Android:
                        throw new Exception( $"Not Supported Platform :: {target}" );
                        break;

                    case BuildTarget.iOS:
                        targetGroup = BuildTargetGroup.iOS;
                        domain = new iOSRomBuilder();
                        break;

                    default:
                        throw new Exception( $"Invalid Platform :: {target}" );
                }

                var application = new RomBuildContext(domain);
                var result = await application.ExecuteRomBuild(targetGroup);
                if( result == false )
                {
                    throw new Exception( "ROM Build Failed" );
                }
                Debug.Log( "ROM Build Success" );
                return true;
            }
            catch( Exception ex )
            {
                Debug.LogError( ex.Message );
                return false;
            }
        }
    }
}
//#endif
