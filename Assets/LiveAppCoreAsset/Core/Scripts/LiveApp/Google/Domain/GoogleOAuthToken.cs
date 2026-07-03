using System;
using UnityEngine;

namespace LiveAppCore.Google.Domain
{
    public class GoogleOAuthToken
    {
        public string accessToken;
        public string refreshToken;
        public long expiresAtUnixTime;
        public string scope;
        public string tokenType;

        public bool HasValidAccessToken()
        {
            if( string.IsNullOrEmpty( accessToken ) )
            {
                return false;
            }

            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // 만료 60초 전부터는 갱신 대상.
            return expiresAtUnixTime > now + OAuthConstValue.TimeSpanSecond;
        }
    }
}
