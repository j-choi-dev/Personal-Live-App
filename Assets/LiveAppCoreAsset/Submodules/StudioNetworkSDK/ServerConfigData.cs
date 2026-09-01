using LiveApp.Util;
using StudioNetworkSDK.Domain;
using System;
using UnityEngine;

namespace StudioNetworkSDK.Infrastructure
{
    /// <summary>
    /// MqTT 서버 Config 정보 관련 구현체 Class
    /// </summary>
    public class ServerConfigData : IMqTTServerConfigDomain
    {
        public MqTTServerConfig GetConfigData()
        {
            var config = new MqTTServerConfig();
            if( PlayerPrefsUtil.IsExistKey( ServerConfigKey.GUID) == false)
            {
                var guid = Guid.NewGuid().ToString();
                PlayerPrefsUtil.SetStringValueByKey( ServerConfigKey.GUID, guid );
            }
            config.guid = PlayerPrefs.GetString( ServerConfigKey.GUID );

            var platform = UnityEngine.Application.platform.ToString();
            if( PlayerPrefs.HasKey( ServerConfigKey.Platform ) == false )
            {
                PlayerPrefs.SetString( ServerConfigKey.Platform, platform );
            }
            config.platform = PlayerPrefs.GetString( ServerConfigKey.Platform );
            return config;
        }
    }
}
