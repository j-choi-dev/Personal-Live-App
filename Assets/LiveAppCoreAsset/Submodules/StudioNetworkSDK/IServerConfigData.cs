using UnityEngine;

namespace StudioNetworkSDK.Domain
{
    public interface IServerConfigDomain
    {
        MqTTServerConfig GetConfigData();
    }
}
