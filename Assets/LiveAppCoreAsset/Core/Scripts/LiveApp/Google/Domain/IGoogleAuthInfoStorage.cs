using UnityEngine;

namespace LiveAppCore.Google.Domain
{
    public interface IGoogleAuthInfoStorage
    {
        GoogleOAuthSettings AuthSetting { get; }
        string Token { get; }

        void SetOAuthSettings( GoogleOAuthSettings setting );
        void SetOAuthToken( string token );
    }
}
