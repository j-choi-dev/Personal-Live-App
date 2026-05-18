using UnityEngine;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Text;
using System;
using UnityEngine.InputSystem;
using StudioNetworkSDK.Domain;

namespace LiveAppCore.Test
{
    public class MqttLoginTester : MonoBehaviour
    {
        private MqttClient client;
        private string clientId;

        private string responseMessage = "";
        private bool isMessageReceived = false;

        void Start()
        {
            clientId = Guid.NewGuid().ToString();
            client = new MqttClient( ServerValue.Address );
            client.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;

            string connectionId = Guid.NewGuid().ToString();
            client.Connect( connectionId );

            string myResponseTopic = $"login/response/{clientId}";
            client.Subscribe( new string[] { myResponseTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE } );

            Debug.Log( "서버 접속 및 수신 대기 완료" );
        }

        void Update()
        {
            if( Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame )
            {
                SendLoginRequest( "testuser", "1234" );
            }

            if( isMessageReceived )
            {
                ProcessResponse();
                isMessageReceived = false;
            }
        }

        public void SendLoginRequest( string id, string pw )
        {
            if( client == null || !client.IsConnected )
            {
                Debug.LogError( "서버에 연결되어 있지 않아! 퍼블릭 IP나 포트를 다시 확인해." );
                return;
            }

            var req = new LoginRequestTemp { clientid = clientId, id = id, password = pw };
            string json = JsonUtility.ToJson(req);

            client.Publish( "login/request", Encoding.UTF8.GetBytes( json ), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false );
            Debug.Log( $"로그인 데이터 전송함 (클라이언트 ID: {clientId})" );
        }

        private void Client_MqttMsgPublishReceived( object sender, MqttMsgPublishEventArgs e )
        {
            responseMessage = Encoding.UTF8.GetString( e.Message );
            isMessageReceived = true;
        }

        private void ProcessResponse()
        {
            LoginResponse res = JsonUtility.FromJson<LoginResponse>(responseMessage);

            if( res.status == "success" )
            {
                Debug.Log( $"로그인 통과! 서버 응답: {res.message}" );
            }
            else
            {
                Debug.LogError( $"로그인 거부: {res.message}" );
            }
        }

        void OnApplicationQuit()
        {
            if( client != null && client.IsConnected )
            {
                client.Disconnect();
            }
        }
    }
}
