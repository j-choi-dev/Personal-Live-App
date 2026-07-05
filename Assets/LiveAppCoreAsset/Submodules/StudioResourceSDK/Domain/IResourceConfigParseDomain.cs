using StudioNetworkSDK.Domain;
using System.Collections.Generic;

namespace StudioResourceSDK.Domain
{
    public interface IResourceConfigParseDomain
    {
        IReadOnlyCollection<ResourceServerData> ResourceServerDatas { get; }

        IReadOnlyCollection<ResourceServerData> ParseServerConfigData( string rawData );
    }
}
