using Cysharp.Threading.Tasks;
using LiveAppCore.Editor.Domain;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using System;
using StudioCharacterSDK.Domain;
using StudioResourceSDK.Domain;

namespace LiveAppCore.Editor.Infrastructure
{
    public class StanaloneAssetBundleBuilder : IAssetBundleBuildDomain
    {
        private readonly BuildTarget _target;
        private List<string> _guids = new List<string>();
        private List<AssetBundleBuild> _buildMap = new List<AssetBundleBuild>();

        private string _rawBuildDirectory;
        private string _publishDirectory;

        public StanaloneAssetBundleBuilder( BuildTarget target )
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

                foreach( string guid in _guids )
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath( guid );
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>( assetPath );

                    if( prefab == null )
                    {
                        continue;
                    }
                    var charcacter = prefab.GetComponent<ICharacter>();
                    if( charcacter != null )
                    {
                        var build = new AssetBundleBuild();
                        build.assetBundleName = Path.Combine( ResourceType.Character.ToString(), prefab.name + ".ab" ).ToLower();
                        build.assetNames = new[] { assetPath };
                        _buildMap.Add( build );
                        Debug.Log( $"{prefab.name} :: {assetPath}, {build.assetBundleName}" );
                    }
                    var bg = prefab.GetComponent<IBackground>();
                    if( bg != null )
                    {
                        var build = new AssetBundleBuild();
                        build.assetBundleName = Path.Combine( ResourceType.BackGround.ToString(), prefab.name + ".ab" ).ToLower();
                        build.assetNames = new[] { assetPath };
                        _buildMap.Add( build );
                        Debug.Log( $"{prefab.name} :: {assetPath}, {build.assetBundleName}" );
                    }
                }

                if( _buildMap.Count == 0 )
                {
                    Debug.LogError( "Can Not Find Exist Any Component Assets" );
                    return false;
                }

                Debug.Log( $"AssetBundle PreProcess Success. Target={_target}, Count={_buildMap.Count}" );
                return true;
            }
            catch( Exception exception )
            {
                Debug.LogError( $"AssetBundle PreProcess Failed: {exception}" );
                return false;
            }
        }

        public async UniTask<bool> BuildProcess( BuildTargetGroup platform )
        {
            try
            {
                string projectRoot = Directory.GetParent( UnityEngine.Application.dataPath ).FullName;
                string platformFolder = GetPlatformFolder( _target );
                string buildTime = DateTime.Now.ToString( "yyyyMMdd_HHmmss" );
                string versionRoot = Path.Combine( projectRoot, "Builds", "AssetBundles", BuildPath.BundleVersion );

                _rawBuildDirectory = Path.Combine( versionRoot, "Raw", platformFolder, buildTime );
                _publishDirectory = Path.Combine( versionRoot, platformFolder );

                Directory.CreateDirectory( _rawBuildDirectory );

                var buildParameters = new BuildAssetBundlesParameters
                {
                    outputPath = _rawBuildDirectory,
                    bundleDefinitions = _buildMap.ToArray(),
                    options = BuildAssetBundleOptions.ChunkBasedCompression,
                    targetPlatform = _target,
                    extraScriptingDefines = new[] { "LIVEAPP_ASSETBUNDLE_STANDALONE" }
                };

                AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles( buildParameters );

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
