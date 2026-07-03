using UnityEngine;

namespace LiveAppCore.Google.Domain
{
    public class GoogleAuthInfoStorage : IGoogleAuthInfoStorage
    {
        public GoogleOAuthSettings AuthSetting { get; private set; }

        public string Token { get; private set; }

        public void SetOAuthSettings( GoogleOAuthSettings setting )
        {
            AuthSetting = setting;
        }

        public void SetOAuthToken( string token )
        {
            Token = token;
        }
    }
}
