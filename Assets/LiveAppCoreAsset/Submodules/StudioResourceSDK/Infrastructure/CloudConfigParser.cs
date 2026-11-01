using StudioResourceSDK.Domain;
using System;

namespace StudioResourceSDK.Infrastructure
{
    public class CloudConfigParser : ICloudConfigParseDomain
    {
        private int AccessKeyIndex = 0;
        private int SecretAccessKeyIndex = 1;

        public CloudConfigData ParseData( string rawData )
        {
            var rows = rawData.Split( new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries );
            var cols = rows[1].Split(',');
            return new CloudConfigData( cols[AccessKeyIndex], cols[SecretAccessKeyIndex] );
        }
    }
}
