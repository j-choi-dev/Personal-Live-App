using StudioNetworkSDK.Domain;
using StudioResourceSDK.Domain;
using System.Collections.Generic;
using UnityEngine;

namespace StudioResourceSDK.Application
{
    public interface IResourceConfigContext
    {
        IReadOnlyCollection<ResourceServerData> ParseServerConfigData(string rawData);
    }
}
