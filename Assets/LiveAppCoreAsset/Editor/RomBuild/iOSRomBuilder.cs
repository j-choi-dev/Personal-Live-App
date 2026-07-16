using Codice.Utils;
using Cysharp.Threading.Tasks;
using LiveAppCore.Editor.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace LiveAppCore.Editor.Infrastructure
{
    /// <summary>
    /// iOS ROM Build 스크립트
    /// </summary>
    /// <remarks>일부 API는 지원 중단 예정일 가능성 높음. @Choi 26.07.14</remarks>
    public class iOSRomBuilder : IRomBuildDomain
    {
        private const BuildTarget Target = BuildTarget.iOS;
        private const BuildTargetGroup TargetGroup = BuildTargetGroup.iOS;

        // TODO 이하의 내용들은 Build Data 등으로 별도 관리 필요.(Scriptable Object나 Text File) @Choi 26.07.14
        private const string BundleIdentifier = "com.weavr.liveappcore";
        private const string CompanyName = "WeaVR";
        private const string ProductName = "Personal Live App";
#if UNITY_EDITOR_WIN
        private const string BuildRootDirectory =  @"C:\Build\iOS";
#else
        private const string BuildRootDirectory = "Builds/iOS";
#endif

        private const string GoogleServicePlistSourcePath = "Assets/Plugins/iOS/GoogleService-Info.plist";

        private const string AppIconPath = "Assets/Icons/AppIcon/icon-1024.png";
        private const string AppVersion = "1.0.0";
        private const string TargetOSVersion = "15.0";

        private const string GoogleServiceInfoPList = "GoogleService-Info.plist";

        private static readonly string[] RequiredScenePaths =
        {
            "Assets/LiveAppCoreAsset/Core/Scenes/LiveAppCore.unity",
            "Assets/LiveAppCoreAsset/Core/Scenes/LiveAppUI.unity"
        };

        private string _projectDirectoryName;
        private string _buildNumber;

        private string XcodeProjectPath => Path.Combine( BuildRootDirectory, _projectDirectoryName );

        private bool _isResult = false;

        public iOSRomBuilder()
        {
            _buildNumber= DateTime.Now.ToString( "yyyyMMddHHmmss" );
            _projectDirectoryName = $"PLA_{_buildNumber}";
        }
        public async UniTask<bool> PreProcess( BuildTargetGroup platform )
        {
            try
            {
                if( platform != TargetGroup )
                {
                    Debug.LogError( $"Invalid platform. Expected: {TargetGroup}, Actual: {platform}" );
                    return false;
                }

                if( EditorUserBuildSettings.activeBuildTarget != Target )
                {
                    Debug.LogError( $"Invalid activeBuildTarget. Expected: {Target}, Actual: {EditorUserBuildSettings.activeBuildTarget}" );
                    return false;
                }

                var switched = EditorUserBuildSettings.SwitchActiveBuildTarget( TargetGroup, Target );

                if( switched == false )
                {
                    Debug.LogError( "Failed to switch active build target to iOS." );
                    return false;
                }

                PlayerSettings.companyName = CompanyName;
                PlayerSettings.productName = ProductName;
                PlayerSettings.SetApplicationIdentifier( NamedBuildTarget.iOS, BundleIdentifier );
                PlayerSettings.bundleVersion = AppVersion;
                PlayerSettings.iOS.targetOSVersionString = TargetOSVersion;
                // PlayerSettings.iOS.appleDeveloperTeamID = "YOUR_TEAM_ID";
                // PlayerSettings.iOS.appleEnableAutomaticSigning = true;

                PlayerSettings.iOS.buildNumber = _buildNumber;

                SetAppIconsIfExists( AppIconPath, platform );

                EnsureRequiredScenes();

                ValidateGoogleServicePlist();

                if( Directory.Exists( BuildRootDirectory ) == false )
                {
                    Directory.CreateDirectory( BuildRootDirectory );
                }
                if( Directory.Exists( XcodeProjectPath ) )
                {
                    Debug.Log( $"[iOSRomBuilder] Delete old Xcode project: {XcodeProjectPath}" );
                    Directory.Delete( XcodeProjectPath, recursive: true );
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log( "[iOSRomBuilder] PreProcess completed." );

                _isResult = true;
            }
            catch( Exception e )
            {
                Debug.LogException( e );
                ResetBuildSettings();
                _isResult = false;
            }
            return _isResult;
        }

        public async UniTask<bool> BuildProcess( BuildTargetGroup platform )
        {
            try
            {
                var enabledScenes = EditorBuildSettings.scenes
                    .Where(scene => scene.enabled)
                    .Select(scene => scene.path)
                    .ToArray();

                if( enabledScenes.Length <= 0 )
                {
                    throw new InvalidOperationException( "No enabled scenes found in Build Settings." );
                }

                var options = new BuildPlayerOptions
                {
                    scenes = enabledScenes,
                    locationPathName = XcodeProjectPath,
                    target = Target,
                    targetGroup = TargetGroup,
                    options = BuildOptions.None
                };

                Debug.Log( $"[iOSRomBuilder] Build started. Path: {XcodeProjectPath}" );
                
                var report = BuildPipeline.BuildPlayer(options);
                var summary = report.summary;
                // TODO Build Log 남기는 것도 좋을 듯 @Choi 26.07.14

                Debug.Log( $"[iOSRomBuilder] Build finished. Result: {summary.result}, TotalSize: {summary.totalSize}, TotalTime: {summary.totalTime}" );

                if( summary.result != BuildResult.Succeeded )
                {
                    throw new Exception( $"iOS Xcode project build failed. Result: {summary.result}" );
                }
                _isResult = true;
                return _isResult;
            }
            catch( Exception e )
            {
                Debug.LogException( e );
                ResetBuildSettings();
                _isResult = false;
            }
            return _isResult;
        }

        public async UniTask<bool> PostProcess( BuildTargetGroup platform )
        {
            try
            {
                if( Directory.Exists( XcodeProjectPath ) == false )
                {
                    throw new DirectoryNotFoundException( $"Xcode project directory not found: {XcodeProjectPath}" );
                }
                string destinationPath = Path.Combine( XcodeProjectPath, GoogleServiceInfoPList);
                File.Copy( GoogleServicePlistSourcePath, destinationPath, overwrite: true );

                ApplyInfoPlistSettings();
                ApplyPbxProjectSettings();

                Debug.Log( "[iOSRomBuilder] PostProcess completed." );
                _isResult = true;
            }
            catch( Exception e )
            {
                Debug.LogException( e );
                _isResult = false;
            }
            finally
            {
                ResetBuildSettings();
            }
            return _isResult;
        }

        private static void SetAppIconsIfExists( string path, BuildTargetGroup target )
        {
            if( File.Exists( path ) == false )
            {
                Debug.LogWarning( $"[iOSRomBuilder] AppIcon file not found. Skip icon setup: {AppIconPath}" );
                return;
            }

            var sourceIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(path); 
            if( sourceIcon == null )
            {
                Debug.LogError( $"[iOSRomBuilder] Failed to load icon as Texture2D: {path}" );
                return;
            }

            var iconSizes = PlayerSettings.GetIconSizes(NamedBuildTarget.iOS, IconKind.Application);
            if( iconSizes == null || iconSizes.Length <= 0 )
            {
                Debug.LogWarning( $"[iOSRomBuilder] No icon sizes found for target: {target}" );
                return;
            }

            if( sourceIcon.width != 1024 || sourceIcon.height != 1024 )
            {
                Debug.LogError( $"[iOSRomBuilder] Recommended source icon size is 1024x1024. Current: {sourceIcon.width}x{sourceIcon.height}, Path: {path}" );
                return;
            }

            Texture2D[] icons = new Texture2D[iconSizes.Length];
            Debug.Log(icons.Length);

            for( int i = 0; i < icons.Length; i++ )
            {
                icons[i] = sourceIcon;
            }

            PlayerSettings.SetIcons( NamedBuildTarget.iOS, icons, IconKind.Application );

            EditorUtility.SetDirty( AssetDatabase.LoadAllAssetsAtPath( "ProjectSettings/ProjectSettings.asset" ).FirstOrDefault() );
            AssetDatabase.SaveAssets();

            Debug.Log( $"[iOSRomBuilder] App icons set from single source. Target: {target}, Source: {path}, Slots: {icons.Length}" );
        }

        private static void EnsureRequiredScenes()
        {
            var currentScenes = EditorBuildSettings.scenes.ToList();
            var paths = currentScenes.Select(scene => scene.path);
            var knownScenePaths = new HashSet<string>( paths );

            bool changed = false;

            foreach( string scenePath in RequiredScenePaths )
            {
                if( File.Exists( scenePath ) == false )
                {
                    throw new FileNotFoundException( $"Required scene not found: {scenePath}" );
                }

                if( knownScenePaths.Contains( scenePath ) )
                {
                    var index = currentScenes.FindIndex(scene => scene.path == scenePath);
                    if( index >= 0 && currentScenes[index].enabled == false )
                    {
                        currentScenes[index] = new EditorBuildSettingsScene( scenePath, true );
                        changed = true;
                        Debug.Log( $"[iOSRomBuilder] Enabled scene: {scenePath}" );
                    }
                    continue;
                }

                currentScenes.Add( new EditorBuildSettingsScene( scenePath, true ) );
                changed = true;

                Debug.Log( $"[iOSRomBuilder] Added scene to Build Settings: {scenePath}" );
            }

            if( changed )
            {
                EditorBuildSettings.scenes = currentScenes.ToArray();
            }
        }

        private static void ValidateGoogleServicePlist()
        {
            if( File.Exists( GoogleServicePlistSourcePath ) == false )
            {
                throw new FileNotFoundException( $"GoogleService-Info.plist not found: {GoogleServicePlistSourcePath}" );
            }

            var googlePlist = new PlistDocument();
            googlePlist.ReadFromFile( GoogleServicePlistSourcePath );

            string clientId = googlePlist.root["CLIENT_ID"]?.AsString();
            string reversedClientId = googlePlist.root["REVERSED_CLIENT_ID"]?.AsString();

            if( string.IsNullOrWhiteSpace( clientId ) )
            {
                throw new Exception( "CLIENT_ID is empty in GoogleService-Info.plist." );
            }
            if( string.IsNullOrWhiteSpace( reversedClientId ) )
            {
                throw new Exception( "REVERSED_CLIENT_ID is empty in GoogleService-Info.plist." );
            }
            Debug.Log( "[iOSRomBuilder] GoogleService-Info.plist validated." );
        }

        private void ApplyInfoPlistSettings()
        {
            var infoPlistPath = Path.Combine(XcodeProjectPath, "Info.plist");
            if( File.Exists( infoPlistPath ) == false )
            {
                throw new FileNotFoundException( $"Info.plist not found: {infoPlistPath}" );
            }

            var googlePlist = new PlistDocument();
            googlePlist.ReadFromFile( GoogleServicePlistSourcePath );

            var clientId = googlePlist.root["CLIENT_ID"].AsString();
            var reversedClientId = googlePlist.root["REVERSED_CLIENT_ID"].AsString();

            var infoPlist = new PlistDocument();
            infoPlist.ReadFromFile( infoPlistPath );

            var root = infoPlist.root;
            root.SetString( "GIDClientID", clientId );
            // TODO 회사 도메인 계정 힌트. 강제 검증은 앱 로직에서 id_token hd claim으로 별도 처리 필요할 듯 @Choi 26.07.14
            root.SetString( "GIDHostedDomain", "www.weavr-corp.com" );

            AddUrlSchemeIfNeeded( root, reversedClientId );
            infoPlist.WriteToFile( infoPlistPath );
            Debug.Log( "[iOSRomBuilder] Info.plist updated." );
        }

        private static void AddUrlSchemeIfNeeded( PlistElementDict root, string urlScheme )
        {
            PlistElementArray urlTypes;

            if( root.values.ContainsKey( "CFBundleURLTypes" ) )
            {
                urlTypes = root["CFBundleURLTypes"].AsArray();
            }
            else
            {
                urlTypes = root.CreateArray( "CFBundleURLTypes" );
            }

            if( ContainsUrlScheme( urlTypes, urlScheme ) )
            {
                return;
            }

            var urlType = urlTypes.AddDict();
            // Xcode identifier 추가.
            urlType.SetString( "CFBundleURLName", BundleIdentifier );

            var schemes = urlType.CreateArray("CFBundleURLSchemes");
            schemes.AddString( urlScheme );
        }

        private static bool ContainsUrlScheme( PlistElementArray urlTypes, string scheme )
        {
            foreach( PlistElement element in urlTypes.values )
            {
                var dict = element.AsDict();
                if( dict == null || dict.values.ContainsKey( "CFBundleURLSchemes" ) == false )
                {
                    continue;
                }
                var schemes = dict["CFBundleURLSchemes"].AsArray();
                foreach( PlistElement schemeElement in schemes.values )
                {
                    if( schemeElement.AsString() == scheme )
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void ApplyPbxProjectSettings()
        {
            var pbxPath = PBXProject.GetPBXProjectPath(XcodeProjectPath);
            if( File.Exists( pbxPath ) == false )
            {
                throw new FileNotFoundException( $"project.pbxproj not found: {pbxPath}" );
            }

            var pbx = new PBXProject();
            pbx.ReadFromFile( pbxPath );

            var mainTargetGuid = pbx.GetUnityMainTargetGuid();
            var frameworkTargetGuid = pbx.GetUnityFrameworkTargetGuid();

            AddGoogleServicePlistToMainTarget( pbx, mainTargetGuid );
            ApplyBuildProperties( pbx, mainTargetGuid, frameworkTargetGuid );
            AddFrameworks( pbx, frameworkTargetGuid );
            pbx.WriteToFile( pbxPath );

            Debug.Log( "[iOSRomBuilder] PBXProject updated." );
        }

        private void AddGoogleServicePlistToMainTarget( PBXProject pbx, string mainTargetGuid )
        {
            string fileGuid = pbx.AddFile( GoogleServiceInfoPList, GoogleServiceInfoPList, PBXSourceTree.Source );
            pbx.AddFileToBuild( mainTargetGuid, fileGuid );
        }

        private static void ApplyBuildProperties( PBXProject pbx, string mainTargetGuid, string frameworkTargetGuid )
        {
            // GoogleSignIn / Pods 사용 시 필요할 수 있는 기본값들.
            pbx.SetBuildProperty( mainTargetGuid, "ENABLE_BITCODE", "NO" );
            pbx.SetBuildProperty( frameworkTargetGuid, "ENABLE_BITCODE", "NO" );

            pbx.SetBuildProperty( mainTargetGuid, "IPHONEOS_DEPLOYMENT_TARGET", "13.0" );
            pbx.SetBuildProperty( frameworkTargetGuid, "IPHONEOS_DEPLOYMENT_TARGET", "13.0" );

            // Swift Package / CocoaPods 사용 시 Swift runtime 관련 문제를 줄이는 옵션.
            pbx.SetBuildProperty( mainTargetGuid, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "YES" );
        }

        private static void AddFrameworks( PBXProject pbx, string frameworkTargetGuid )
        {
            // TODO GoogleSignIn SDK 자체는 CocoaPods/SPM이 링크하는 것이 나을 듯 @Choi 26.07.14
            // TODO framework 후보가 적절한지 MacOS/iPhone에서 검증 필요 @Choi 26.07.14
            pbx.AddFrameworkToProject( frameworkTargetGuid, "AuthenticationServices.framework", false );
            pbx.AddFrameworkToProject( frameworkTargetGuid, "SafariServices.framework", false );
            pbx.AddFrameworkToProject( frameworkTargetGuid, "Security.framework", false );
            pbx.AddFrameworkToProject( frameworkTargetGuid, "SystemConfiguration.framework", false );
        }

        private static void ResetBuildSettings()
        {
            // 수동 빌드가 안 되도록 빌드 설정을 초기화.
            PlayerSettings.companyName = string.Empty;
            PlayerSettings.productName = string.Empty;
            PlayerSettings.SetApplicationIdentifier( NamedBuildTarget.iOS, string.Empty );
            PlayerSettings.bundleVersion = string.Empty;
            PlayerSettings.iOS.buildNumber = "0";
            Texture2D[] emptyIcons = new Texture2D[0];
            PlayerSettings.SetIcons( NamedBuildTarget.iOS, emptyIcons, IconKind.Application );
            Debug.Log( "Reset Complete" );
        }
    }
}
