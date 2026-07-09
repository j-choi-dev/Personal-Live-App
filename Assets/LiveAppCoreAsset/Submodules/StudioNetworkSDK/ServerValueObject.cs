using StudioResourceSDK.Domain;
using System;
using UnityEngine;

namespace StudioNetworkSDK.Domain
{
    public static class ServerValue
    {
        public static readonly string Address = "ec2-3-35-8-241.ap-northeast-2.compute.amazonaws.com";
    }
    
    
    [Serializable]
    public class MqTTServerConfig
    {
        public string guid;
        public string platform;
    }

    [Serializable]
    public class LoginRequest
    {
        public MqTTServerConfig config;
        public string id;
        public string password;
    }

    [Serializable]
    public class LoginRequestTemp
    {
        public string clientid;
        public string id;
        public string password;
    }

    [Serializable]
    public class LoginResponse
    {
        public string status;
        public string message;
    }
    
    public static class ServerConfigKey
    {
        public static string GUID = "Client_Guid";
        public static string Platform = "Platform";
    }

    public enum ServerType
    {
        None = 0,
        Develop,
        Staging,
        Master,
    }

    /// <summary>
    /// MqTT Message Data Class
    /// </summary>
    public class MqttMessageData
    {
        public string Topic { get; set; }
        public string Payload { get; set; }
    }

    /// <summary>
    /// Protocol 정의
    /// </summary>
    public enum NetworkProtocol
    {
        Unknown,
        LoginResponse,
        RoomJoinResponse,
    }

    public class ResourceServerData
    {
        public readonly ResourceType resourceType = ResourceType.None;
        public readonly ServerType serverType = ServerType.None;
        public readonly string tableUrl = string.Empty;
        public readonly string tableGid = string.Empty;

        public ResourceServerData( ResourceType resourceType,
            ServerType serverType,
            string tableUrl,
            string tableGid )
        {
            this.resourceType = resourceType;
            this.serverType = serverType;
            this.tableUrl = tableUrl;
            this.tableGid = tableGid;
        }
    }
}
