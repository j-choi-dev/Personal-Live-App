using Cysharp.Threading.Tasks;
using LiveAppCore.Editor.Domain;
using StudioCharacterSDK.Domain;
using StudioResourceSDK.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace LiveAppCore.Editor.View
{
    public class IOSAssetBundleBuilder : IAssetBundleBuildDomain
    {
        private readonly BuildTarget _target;
        private List<string> _guids = new List<string>();
        private List<AssetBundleBuild> _buildMap = new List<AssetBundleBuild>();

        private string _rawBuildDirectory;
        private string _publishDirectory;

        public IOSAssetBundleBuilder( BuildTarget target )
        {
            _target = target;
        }

        public async UniTask<bool> PreProcess( BuildTargetGroup platform )
        {
            try
            {
                _guids.Clear();
                _buildMap.Clear();
                _guids = AssetDatabase.FindAssets( "t:Prefab", new[] { BuildPath.OriginalCharacterBundleRoot, BuildPath.OriginalBackGroundBundleRoot } ).ToList();

                foreach(var guid in _guids)
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                    if(prefab == null)
                    {
                        continue;
                    }
                    var charcacter = prefab.GetComponent<ICharacter>();
                    if( charcacter != null )
                    {
                        var build = new AssetBundleBuild(); 
                        build.assetBundleName = $"{ResourceType.Character.ToString().ToLowerInvariant()}/{prefab.name.ToLowerInvariant()}.ab";
                        build.assetNames = new[] { assetPath };
                        _buildMap.Add( build );
                        Debug.Log( $"{prefab.name} :: {assetPath}, {build.assetBundleName}" );
                    }
                    var bg = prefab.GetComponent<IBackground>();
                    if( bg != null )
                    {
                        var build = new AssetBundleBuild();
                        build.assetBundleName = $"{ResourceType.BackGround.ToString().ToLowerInvariant()}/{prefab.name.ToLowerInvariant()}.ab";
                        build.assetNames = new[] { assetPath };
                        _buildMap.Add( build );
                        Debug.Log( $"{prefab.name} :: {assetPath}, {build.assetBundleName}" );
                    }
                }

                if(_buildMap.Count == 0)
                {
                    Debug.LogError( "Can Not Find Exist Any Component Assets" );
                    return false;
                }
                Debug.Log( $"AssetBundle PreProcess Successed" );
                return true;
            }
            catch(Exception e)
            {
                Debug.LogError( $"AssetBundle PreProcess Failed: {e.Message}" );
                return false;
            }
        }

        public async UniTask<bool> BuildProcess( BuildTargetGroup platform )
        {
            try
            {
                string projectRoot = Directory.GetParent( UnityEngine.Application.dataPath ).FullName;
                string buildTime = DateTime.Now.ToString( "yyyyMMdd_HHmmss" );
                string versionRoot = Path.Combine( projectRoot, "Builds", "AssetBundles", BuildPath.BundleVersion );

                _rawBuildDirectory = Path.Combine( versionRoot, "Raw", "iOS", buildTime );
                _publishDirectory = Path.Combine( versionRoot, "iOS" );

                Directory.CreateDirectory( _rawBuildDirectory );

                AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles( _rawBuildDirectory, _buildMap.ToArray(), BuildAssetBundleOptions.ChunkBasedCompression, _target );

                if( manifest == null )
                {
                    Debug.LogError( $"AssetBundle Build Failed. Target={_target}" );
                    return false;
                }

                string manifestFilePath = Path.Combine( _rawBuildDirectory, "BundleManifestInfo.txt" );

                using( var writer = new StreamWriter( manifestFilePath ) )
                {
                    foreach( string bundleName in manifest.GetAllAssetBundles() )
                    {
                        string bundleHash = manifest.GetAssetBundleHash( bundleName ).ToString();
                        writer.WriteLine( $"{bundleName}/{bundleHash}" );
                    }
                }

                Debug.Log( $"AssetBundle Raw Build Success. Version={BuildPath.BundleVersion}, Target={_target}, Count={_buildMap.Count}, Path={_rawBuildDirectory}" );
                return true;
            }
            catch( Exception exception )
            {
                Debug.LogError( $"AssetBundle BuildProcess Failed: {exception}" );
                return false;
            }
        }


        public async UniTask<bool> PostProcess( BuildTargetGroup platform )
        {
            string temporaryDirectory = null;

            try
            {
                if( string.IsNullOrWhiteSpace( _rawBuildDirectory ) || Directory.Exists( _rawBuildDirectory ) == false )
                {
                    throw new DirectoryNotFoundException( $"Raw AssetBundle 경로를 찾을 수 없습니다. Path={_rawBuildDirectory}" );
                }

                if( string.IsNullOrWhiteSpace( _publishDirectory ) )
                {
                    throw new InvalidOperationException( "AssetBundle Publish 경로가 설정되지 않았습니다." );
                }

                temporaryDirectory = _publishDirectory + ".tmp";

                if( Directory.Exists( temporaryDirectory ) )
                {
                    Directory.Delete( temporaryDirectory, true );
                }

                CopyDirectory( _rawBuildDirectory, temporaryDirectory );

                if( Directory.Exists( _publishDirectory ) )
                {
                    Directory.Delete( _publishDirectory, true );
                }

                Directory.Move( temporaryDirectory, _publishDirectory );

                Debug.Log( $"AssetBundle PostProcess Success. Raw={_rawBuildDirectory}, Publish={_publishDirectory}" );
                return true;
            }
            catch( Exception exception )
            {
                if( string.IsNullOrWhiteSpace( temporaryDirectory ) == false && Directory.Exists( temporaryDirectory ) )
                {
                    Directory.Delete( temporaryDirectory, true );
                }

                Debug.LogError( $"AssetBundle PostProcess Failed: {exception}" );
                return false;
            }
        }

        private static void CopyDirectory( string sourceDirectory, string destinationDirectory )
        {
            Directory.CreateDirectory( destinationDirectory );

            foreach( string filePath in Directory.GetFiles( sourceDirectory ) )
            {
                string destinationPath = Path.Combine( destinationDirectory, Path.GetFileName( filePath ) );
                File.Copy( filePath, destinationPath, true );
            }

            foreach( string directoryPath in Directory.GetDirectories( sourceDirectory ) )
            {
                string destinationPath = Path.Combine( destinationDirectory, Path.GetFileName( directoryPath ) );
                CopyDirectory( directoryPath, destinationPath );
            }
        }

        private static string GetPlatformFolder( BuildTarget target )
        {
            switch( target )
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return "Windows";

                case BuildTarget.StandaloneOSX:
                    return "OSX";

                case BuildTarget.iOS:
                    return "iOS";

                default:
                    throw new NotSupportedException( $"지원하지 않는 Standalone Target입니다. Target={target}" );
            }
        }
    }
}
