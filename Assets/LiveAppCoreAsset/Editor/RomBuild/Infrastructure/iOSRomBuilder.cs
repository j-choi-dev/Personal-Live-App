using Cysharp.Threading.Tasks;
using LiveAppCore.Editor.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.iOS;
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
        private IRomBuildConfig _romConfig;
        private IiOSRomBuildConfig _iOSConfig;

        private const BuildTarget Target = BuildTarget.iOS;
        private const BuildTargetGroup TargetGroup = BuildTargetGroup.iOS;

#if UNITY_EDITOR_WIN
        private const string BuildRootDirectory =  @"C:\Build\iOS";
#else
        private const string BuildRootDirectory = "Builds/iOS";
#endif

        private const string PlugInPath = "Assets/Plugins/iOS";
        private const string GoogleServiceInfoPList = "GoogleService-Info.plist";
        private const string AppIconPath = "Assets/Icons/AppIcon/AppIcon-1024.png";

        private static readonly string[] RequiredScenePaths =
        {
            "Assets/LiveAppCoreAsset/Core/Scenes/LiveAppCore.unity",
            "Assets/LiveAppCoreAsset/Core/Scenes/LiveAppUI.unity"
        };

        private string _projectDirectoryName;
        private string _buildNumber;

        private string XcodeProjectPath => Path.Combine( BuildRootDirectory, _projectDirectoryName );

        private bool _isResult = false;

        public iOSRomBuilder( IRomBuildConfig romConfig, IiOSRomBuildConfig iOSConfig )
        {
            _romConfig = romConfig;
            _iOSConfig = iOSConfig;

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

                PlayerSettings.companyName = _romConfig.CompanyName;
                PlayerSettings.productName = _romConfig.ProductName;
                PlayerSettings.SetApplicationIdentifier( NamedBuildTarget.iOS, _romConfig.BundleIdentifier );
                PlayerSettings.bundleVersion = _romConfig.AppVersion;
                PlayerSettings.iOS.targetOSVersionString = _iOSConfig.TargetOSVersion;
                PlayerSettings.iOS.appleDeveloperTeamID = _iOSConfig.TeamID;
                PlayerSettings.iOS.appleEnableAutomaticSigning = true;

                PlayerSettings.iOS.buildNumber = _iOSConfig.BuildNumber.ToString();

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

                var report = UnityEditor.BuildPipeline.BuildPlayer(options);
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
                var pListPath = Path.Combine(PlugInPath, GoogleServiceInfoPList);
                string destinationPath = Path.Combine( XcodeProjectPath, GoogleServiceInfoPList);
                File.Copy( pListPath, destinationPath, overwrite: true );

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
            if( string.IsNullOrWhiteSpace( path ) )
            {
                throw new ArgumentException( "App icon path is empty.", nameof( path ) );
            }

            // 최신 파일 상태를 Unity AssetDatabase에 반영
            AssetDatabase.ImportAsset( path, ImportAssetOptions.ForceUpdate );

            Texture2D sourceIcon = AssetDatabase.LoadAssetAtPath<Texture2D>( path );

            if( sourceIcon == null )
            {
                throw new FileNotFoundException( $"Failed to load app icon: {path}", path );
            }

            if( sourceIcon.width != 1024 ||
                sourceIcon.height != 1024 )
            {
                throw new InvalidOperationException( $"App icon must be 1024x1024. Current={sourceIcon.width}x{sourceIcon.height}, Path={path}" );
            }

            PlatformIcon[] platformIcons = PlayerSettings.GetPlatformIcons( NamedBuildTarget.iOS, iOSPlatformIconKind.Application );

            if( platformIcons == null ||
                platformIcons.Length == 0 )
            {
                throw new InvalidOperationException( "No iOS application icon slots were found." );
            }

            for( int i = 0; i < platformIcons.Length; i++ )
            {
                if( platformIcons[i].maxLayerCount <= 0 )
                {
                    continue;
                }

                platformIcons[i].SetTexture( sourceIcon, 0 );

                Debug.Log(
                    $"[iOSRomBuilder] Icon slot assigned: {platformIcons[i].width} x {platformIcons[i].height}"
                );
            }

            PlayerSettings.SetPlatformIcons( NamedBuildTarget.iOS, iOSPlatformIconKind.Application, platformIcons );

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 실제 적용 여부 검증
            PlatformIcon[] appliedIcons = PlayerSettings.GetPlatformIcons( NamedBuildTarget.iOS, iOSPlatformIconKind.Application );

            bool allAssigned = true;

            foreach( PlatformIcon icon in appliedIcons )
            {
                if( icon.maxLayerCount <= 0 )
                {
                    continue;
                }
                if( icon.GetTexture( 0 ) == null )
                {
                    allAssigned = false;

                    Debug.LogError( $"[iOSRomBuilder] Empty icon slot: {icon.width}x{icon.height}" );
                }
            }

            if( !allAssigned )
            {
                throw new InvalidOperationException( "One or more iOS app icon slots are empty." );
            }

            Debug.Log( $"[iOSRomBuilder] iOS app icon configured. Source={path}, Slots={appliedIcons.Length}" );
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
            var pListPath = Path.Combine(PlugInPath, GoogleServiceInfoPList);
            if( File.Exists( pListPath ) == false )
            {
                throw new FileNotFoundException( $"GoogleService-Info.plist not found: {pListPath}" );
            }

            var googlePlist = new PlistDocument();
            googlePlist.ReadFromFile( pListPath );

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
            var googlePlistPath = Path.Combine(PlugInPath, GoogleServiceInfoPList);
            var mainInfoPlistPath = Path.Combine(XcodeProjectPath, "Info.plist");
            if( File.Exists( mainInfoPlistPath ) == false )
            {
                throw new FileNotFoundException( $"Info.plist not found: {mainInfoPlistPath}" );
            }

            var googlePlist = new PlistDocument();
            googlePlist.ReadFromFile( googlePlistPath );

            var clientId = googlePlist.root["CLIENT_ID"].AsString();
            var reversedClientId = googlePlist.root["REVERSED_CLIENT_ID"].AsString();

            var mainInfoPlist = new PlistDocument();
            mainInfoPlist.ReadFromFile( mainInfoPlistPath );

            var mainRoot = mainInfoPlist.root;

            mainRoot.SetString( "CFBundleShortVersionString", _romConfig.AppVersion );
            mainRoot.SetString( "CFBundleVersion", _iOSConfig.BuildNumber.ToString() );
            mainRoot.SetString( "GIDClientID", clientId );
            // TODO 회사 도메인 계정 힌트. 강제 검증은 앱 로직에서 id_token hd claim으로 별도 처리 필요할 듯 @Choi 26.07.14
            mainRoot.SetString( "GIDHostedDomain", "www.weavr-corp.com" );

            AddUrlSchemeIfNeeded( mainRoot, reversedClientId );
            mainInfoPlist.WriteToFile( mainInfoPlistPath );
            // UnityFramework Info.plist
            string frameworkInfoPlistPath = Path.Combine( XcodeProjectPath, "UnityFramework", "Info.plist" );

            if( File.Exists( frameworkInfoPlistPath ) )
            {
                var frameworkInfoPlist = new PlistDocument();
                frameworkInfoPlist.ReadFromFile( frameworkInfoPlistPath );
                frameworkInfoPlist.root.SetString( "CFBundleShortVersionString", _romConfig.AppVersion );
                frameworkInfoPlist.root.SetString( "CFBundleVersion", _iOSConfig.BuildNumber.ToString() );
                frameworkInfoPlist.WriteToFile( frameworkInfoPlistPath );

                Debug.Log( $"[iOSRomBuilder] UnityFramework version updated. Version={_romConfig.AppVersion}, Build={_iOSConfig.BuildNumber}" );
            }
            else
            {
                Debug.LogError( $"[iOSRomBuilder] UnityFramework Info.plist not found: {frameworkInfoPlistPath}" );
            }

            Debug.Log( $"[iOSRomBuilder] Info.plist updated. Version={_romConfig.AppVersion}, Build={_iOSConfig.BuildNumber}" );
        }

        private void AddUrlSchemeIfNeeded( PlistElementDict root, string urlScheme )
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
            urlType.SetString( "CFBundleURLName", _romConfig.BundleIdentifier );

            var schemes = urlType.CreateArray("CFBundleURLSchemes");
            schemes.AddString( urlScheme );
        }

        private bool ContainsUrlScheme( PlistElementArray urlTypes, string scheme )
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

            AddGoogleServicePlistToMainTarget( pbx, mainTargetGuid, frameworkTargetGuid );
            ApplyBuildProperties( pbx, mainTargetGuid, frameworkTargetGuid );
            AddFrameworks( pbx, frameworkTargetGuid );
            pbx.WriteToFile( pbxPath );

            Debug.Log( "[iOSRomBuilder] PBXProject updated." );
        }

        private void AddGoogleServicePlistToMainTarget( PBXProject pbx, string mainTargetGuid, string frameworkTargetGuid )
        {
            string fileGuid = pbx.AddFile( GoogleServiceInfoPList, GoogleServiceInfoPList, PBXSourceTree.Source );
            pbx.AddFileToBuild( mainTargetGuid, fileGuid );
        }

        private void ApplyBuildProperties( PBXProject pbx, string mainTargetGuid, string frameworkTargetGuid )
        {
            string[] targetGuids = { mainTargetGuid, frameworkTargetGuid };
            // Bitcode 비활성화
            pbx.SetBuildProperty( mainTargetGuid, "ENABLE_BITCODE", "NO" );
            pbx.SetBuildProperty( frameworkTargetGuid, "ENABLE_BITCODE", "NO" );

            // Unity-iPhone과 UnityFramework 모두 OS 버전 지정
            pbx.SetBuildProperty( mainTargetGuid, "IPHONEOS_DEPLOYMENT_TARGET", _iOSConfig.TargetOSVersion );
            pbx.SetBuildProperty( frameworkTargetGuid, "IPHONEOS_DEPLOYMENT_TARGET", _iOSConfig.TargetOSVersion );

            // Unity-iPhone과 UnityFramework의 Version
            pbx.SetBuildProperty( targetGuids, "MARKETING_VERSION", _romConfig.AppVersion );

            // Unity-iPhone과 UnityFramework의 Build
            pbx.SetBuildProperty( targetGuids, "CURRENT_PROJECT_VERSION", _iOSConfig.BuildNumber.ToString() );

            // 자동 서명
            pbx.SetBuildProperty( targetGuids, "CODE_SIGN_STYLE", "Automatic" );
            pbx.SetBuildProperty( targetGuids, "DEVELOPMENT_TEAM", _iOSConfig.TeamID );

            // PBX 프로젝트의 Team 속성도 설정
            pbx.SetTeamId( mainTargetGuid, _iOSConfig.TeamID );
            pbx.SetTeamId( frameworkTargetGuid, _iOSConfig.TeamID );

            // 기존 수동 프로비저닝 값 제거
            pbx.SetBuildProperty( targetGuids, "PROVISIONING_PROFILE_SPECIFIER", string.Empty );

            pbx.SetBuildProperty( targetGuids, "PROVISIONING_PROFILE", string.Empty );

            // Swift 런타임은 메인 앱 타깃에 적용
            pbx.SetBuildProperty( mainTargetGuid, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "YES" );

            // Xcode가 사용할 기본 AppIcon 세트 이름
            pbx.SetBuildProperty( mainTargetGuid, "ASSETCATALOG_COMPILER_APPICON_NAME", "AppIcon" );
        }

        private void AddFrameworks( PBXProject pbx, string frameworkTargetGuid )
        {
            // TODO GoogleSignIn SDK 자체는 CocoaPods/SPM이 링크하는 것이 나을 듯 @Choi 26.07.14
            // TODO framework 후보가 적절한지 MacOS/iPhone에서 검증 필요 @Choi 26.07.14
            pbx.AddFrameworkToProject( frameworkTargetGuid, "AuthenticationServices.framework", false );
            pbx.AddFrameworkToProject( frameworkTargetGuid, "SafariServices.framework", false );
            pbx.AddFrameworkToProject( frameworkTargetGuid, "Security.framework", false );
            pbx.AddFrameworkToProject( frameworkTargetGuid, "SystemConfiguration.framework", false );
        }

        private void ResetBuildSettings()
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
