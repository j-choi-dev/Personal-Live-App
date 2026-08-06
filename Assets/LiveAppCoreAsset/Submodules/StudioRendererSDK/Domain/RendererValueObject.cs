using System;

namespace StudioRendererSDK.Domain
{
    [Serializable]
    public sealed class AgentApiResponse
    {
        public bool success;
        public string message;
        public bool obsRunning;
        public bool launched;
        public string utcTime; public static AgentApiResponse Ok( string message, bool obsRunning, bool launched = false )
        {
            return new AgentApiResponse
            {
                success = true,
                message = message,
                obsRunning = obsRunning,
                launched = launched,
                utcTime = DateTime.UtcNow.ToString( "O" )
            };
        }

        public static AgentApiResponse Error( string message, bool obsRunning )
        {
            return new AgentApiResponse
            {
                success = false,
                message = message,
                obsRunning = obsRunning,
                launched = false,
                utcTime = DateTime.UtcNow.ToString( "O" )
            };
        }
    }

    [Serializable]
    public sealed class VideoSessionRequest
    {
        public string sessionId;
    }

    [Serializable]
    public sealed class VideoSessionDescriptionRequest
    {
        public string sessionId;
        public string type;
        public string sdp;
    }

    [Serializable]
    public sealed class VideoSessionDescriptionResponse
    {
        public bool success;
        public string message;
        public bool hasValue;

        public string sessionId;
        public string type;
        public string sdp;
    }
}