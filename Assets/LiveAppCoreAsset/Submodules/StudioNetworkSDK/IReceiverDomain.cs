using System;
using uPLibrary.Networking.M2Mqtt;

namespace StudioNetworkSDK.Domain
{
    public interface IReceiverDomain
    {
        IObservable<MqttMessageData> OnMessageReceived { get; }
        void Initialize( MqttClient client, MqTTServerConfig config );
    }

    // 수신된 메시지를 담을 DTO (Data Transfer Object)
    public class MqttMessageData
    {
        public string Topic { get; set; }
        public string Payload { get; set; }
    }

    public enum NetworkProtocol
    {
        Unknown,
        LoginResponse,
        // 예: PlayerInfoResponse, RoomJoinResponse 등
    }
}
