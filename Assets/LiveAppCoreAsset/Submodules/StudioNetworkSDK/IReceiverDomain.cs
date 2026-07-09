using System;
using uPLibrary.Networking.M2Mqtt;

namespace StudioNetworkSDK.Domain
{
    /// <summary>
    /// MqTT 서버 수신 관련 Interface
    /// </summary>
    public interface IReceiverDomain
    {
        /// <summary>
        /// 서버로부터 메시지 수신 관련 이벤트
        /// </summary>
        IObservable<MqttMessageData> OnMessageReceived { get; }
        void Initialize( MqttClient client, MqTTServerConfig config );
    }
}
