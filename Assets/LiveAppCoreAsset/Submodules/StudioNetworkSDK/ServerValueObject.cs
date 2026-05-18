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
}
