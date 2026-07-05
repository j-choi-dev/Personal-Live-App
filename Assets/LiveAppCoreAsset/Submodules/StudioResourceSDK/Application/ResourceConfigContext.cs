using StudioNetworkSDK.Domain;
using StudioResourceSDK.Domain;
using System.Collections.Generic;

namespace StudioResourceSDK.Application
{
    public class ResourceConfigContext : IResourceConfigContext
    {
        private IResourceConfigParseDomain _domain;

        public IReadOnlyCollection<ResourceServerData> ParseServerConfigData( string rawData )
        {
            return _domain.ParseServerConfigData(rawData );
        }
    }
}
