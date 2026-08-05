using System;
using System.IO;

namespace StudioSystemSDK.Domain
{
    public class SystemPathValue
    {
        private const string Config = "Config";
        public static string ConfigOriginRoot => Path.Combine( UnityEngine.Application.streamingAssetsPath, Config );
        public static string ConfigDestinationRoot => Path.Combine( UnityEngine.Application.persistentDataPath, Config );
    }

    [Serializable]
    public sealed class AgentApiResponse
    {
        public bool success;
        public string message;
        public bool obsRunning;
        public bool launched;
        public string utcTime; public static AgentApiResponse Ok( string message, bool obsRunning, bool launched = false )
        {
            return new AgentApiResponse {
                success = true,
                message = message,
                obsRunning = obsRunning,
                launched = launched,
                utcTime = DateTime.UtcNow.ToString( "O" )
            };
        }

        public static AgentApiResponse Error( string message, bool obsRunning )
        {
            return new AgentApiResponse {
                success = false,
                message = message,
                obsRunning = obsRunning,
                launched = false,
                utcTime = DateTime.UtcNow.ToString( "O" )
            };
        }
    }
}