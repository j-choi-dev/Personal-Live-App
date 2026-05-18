using Cysharp.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt;

namespace StudioNetworkSDK.Domain
{
    public interface ISenderDomain
    {
        MqttClient GetClient();
        UniTask<bool> Initialize( MqTTServerConfig config );
        UniTask<bool> SendLoginRequest( string id, string pw );
    }
}
