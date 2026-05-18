using Cysharp.Threading.Tasks;
using StudioNetworkSDK.Domain;
using System.Text;
using UniRx;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace StudioNetworkSDK.Infrastructure
{
    public class MqTTSender : ISenderDomain
    {
        private MqttClient _client = null;
        private MqTTServerConfig _config = null;
        public MqttClient GetClient() => _client;

        public async UniTask<bool> Initialize( MqTTServerConfig config )
        {
            _config = config;
            _client = new MqttClient( ServerValue.Address );

            // Sender가 대표로 서버에 연결을 수행해
            _client.Connect( config.guid );

            Debug.Log( "MqTTSender: 서버 접속 완료" );
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
            Debug.Log( $"로그인 데이터 전송함 (클라이언트 ID: {_config.guid})" );
            return true;
        }
    }
}