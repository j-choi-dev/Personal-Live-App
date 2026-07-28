using StudioNetworkSDK.Domain;
using System;
using System.Text;
using UniRx;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace StudioNetworkSDK.Infrastructure
{
    /// <summary>
    /// MqTT 서버 수신 관련 Interface
    /// </summary>
    public class MqTTReceiver : IReceiverDomain, IDisposable
    {
        private readonly Subject<MqttMessageData> _onMessageReceived = new Subject<MqttMessageData>();
        public IObservable<MqttMessageData> OnMessageReceived => _onMessageReceived;
        private MqttClient _client;
        private MqTTServerConfig _config = null;

        public void Initialize( MqttClient client, MqTTServerConfig config )
        {
            _client = client;

            if( _client != null && _client.IsConnected )
            {
                // 1. 이벤트 바인딩
                _client.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;

                // 2. 응답받을 토픽 구독
                string myResponseTopic = $"login/response/{config.guid}";
                _client.Subscribe( new string[] { myResponseTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE } );

                Debug.Log( $"MqTTReceiver: 수신 이벤트 바인딩 및 구독 완료 ({myResponseTopic})" );
            }
            else
            {
                Debug.LogError( "MqTTReceiver: MqttClient가 Null이거나 연결되지 않았어." );
            }
        }

        private void Client_MqttMsgPublishReceived( object sender, MqttMsgPublishEventArgs e )
        {
            try
            {
                var payload = Encoding.UTF8.GetString(e.Message);
                var topic = e.Topic;

                var messageData = new MqttMessageData
                {
                    Topic = topic,
                    Payload = payload
                };

                Debug.Log( $"MqTTReceiver : {messageData.Topic}, {messageData.Payload}" );
                _onMessageReceived.OnNext( messageData );
            }
            catch( Exception ex )
            {
                Debug.LogError( $"MqTTReceiver 파싱 에러: {ex.Message}" );
            }
        }

        public void Dispose()
        {
            if( _client != null )
            {
                _client.MqttMsgPublishReceived -= Client_MqttMsgPublishReceived;
            }

            _onMessageReceived.OnCompleted();
            _onMessageReceived.Dispose();
        }
    }
}
