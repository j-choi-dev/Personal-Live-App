using UnityEngine;

namespace LiveAppCore.Editor.Domain
{
    public interface IRomBuildConfig
    {
        string BundleIdentifier { get; }
        string CompanyName { get; }
        string ProductName { get; }

        string AppVersion { get; }
    }
    public interface IiOSRomBuildConfig
    {
        string TeamID { get; }
        string TargetOSVersion { get; }
        int BuildNumber { get; }
    }
}
