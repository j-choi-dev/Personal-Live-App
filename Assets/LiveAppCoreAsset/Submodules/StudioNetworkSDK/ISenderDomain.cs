using Cysharp.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt;

namespace StudioNetworkSDK.Domain
{
    /// <summary>
    /// MqTT 서버 송신 관련 Interface
    /// </summary>
    public interface ISenderDomain
    {
        MqttClient GetClient();
        UniTask<bool> Initialize( MqTTServerConfig config );
        UniTask<bool> SendLoginRequest( string id, string pw );
    }
}
