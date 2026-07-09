using Cysharp.Threading.Tasks;
using StudioNetworkSDK.Domain;
using System.Text;
using UniRx;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace StudioNetworkSDK.Infrastructure
{
    /// <summary>
    /// MqTT 송신 관련 구현체 Class
    /// </summary>
    public class MqTTSender : ISenderDomain
    {
        private MqttClient _client = null;
        private MqTTServerConfig _config = null;
        public MqttClient GetClient() => _client;

        public async UniTask<bool> Initialize( MqTTServerConfig config )
        {
            _config = config;
            _client = new MqttClient( ServerValue.Address );
            _client.Connect( config.guid );
            return true;
        }

        public async UniTask<bool> SendLoginRequest( string id, string pw )
        {
            if( _client == null || _client.IsConnected == false )
            {
                Debug.LogError( "Not Connected" );
                return false;
            }

            var req = new LoginRequest { config = _config, id = id, password = pw };
            var json = JsonUtility.ToJson(req);

            _client.Publish( "login/request", Encoding.UTF8.GetBytes( json ), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false );
            return true;
        }
    }
}