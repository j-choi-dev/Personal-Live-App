using UnityEngine;

namespace StudioNetworkSDK.Domain
{
    /// <summary>
    /// MqTT 서버 Config 정보 관련 Interface
    /// </summary>
    public interface IMqTTServerConfigDomain
    {
        /// <summary>
        /// MqTT 서버 Config 정보 취득
        /// </summary>
        /// <returns>MqTT 서버 Config 정보</returns>
        MqTTServerConfig GetConfigData();
    }
}
