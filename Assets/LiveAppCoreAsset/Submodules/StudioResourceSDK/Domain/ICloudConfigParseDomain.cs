namespace StudioResourceSDK.Domain
{
    public interface ICloudConfigParseDomain
    {
        CloudConfigData ParseData( string rawData );
    }
}
