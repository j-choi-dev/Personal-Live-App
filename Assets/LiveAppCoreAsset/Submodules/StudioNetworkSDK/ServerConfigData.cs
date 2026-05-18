using StudioNetworkSDK.Domain;
using System;
using UnityEngine;

namespace StudioNetworkSDK.Infrastructure
{
    public class ServerConfigData : IServerConfigDomain
    {
        public MqTTServerConfig GetConfigData()
        {
            var config = new MqTTServerConfig();
            if(PlayerPrefs.HasKey( ServerConfigKey.GUID) == false)
            {
                var guid = Guid.NewGuid().ToString();
                PlayerPrefs.SetString( ServerConfigKey.GUID, guid );
            }
            config.guid = PlayerPrefs.GetString( ServerConfigKey.GUID );

            var platform = UnityEngine.Application.platform.ToString();
            if( PlayerPrefs.HasKey( ServerConfigKey.Platform ) == false )
            {
                PlayerPrefs.SetString( ServerConfigKey.Platform, platform );
            }
            config.platform = PlayerPrefs.GetString( ServerConfigKey.Platform );
            Debug.Log( $"config = {config.guid}" );
            return config;
        }
    }
}
