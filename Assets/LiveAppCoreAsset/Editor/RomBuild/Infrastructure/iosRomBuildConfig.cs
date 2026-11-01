using UnityEngine;
using LiveAppCore.Editor.Domain;

namespace LiveAppCore.Editor.Infrastructure
{
    [CreateAssetMenu( fileName = "RomBuildConfig", menuName = "LiveAppCore/Build/RomBuildConfig" )]
    public class iosRomBuildConfig : ScriptableObject, IRomBuildConfig, IiOSRomBuildConfig
    {
        private const string _bundleIdentifier = "com.weavr-corp.liveapp";
        public string BundleIdentifier => _bundleIdentifier;

        private const string _companyName = "WeaVR";
        public string CompanyName => _companyName;

        private const string _productName = "Personal Live App";
        public string ProductName => _productName;

        private const string _appleDeveloperTeamId = "H95DZH5HFJ";
        public string TeamID => _appleDeveloperTeamId;

        private const string _targetOSVersion = "15.0";
        public string TargetOSVersion => _targetOSVersion;

        [SerializeField] private string _appVersion = "0.0.1";
        public string AppVersion => _appVersion;

        [SerializeField] private int _buildNumber = 1;
        public int BuildNumber => _buildNumber;
    }
}
