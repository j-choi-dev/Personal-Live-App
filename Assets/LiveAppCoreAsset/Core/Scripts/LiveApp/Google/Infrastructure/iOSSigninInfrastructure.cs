using Cysharp.Threading.Tasks;
using LiveAppCore.Google.Domain;
using SimpleJSON;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

namespace LiveAppCore.Google.Infrastructure
{
    public class iOSSigninInfrastructure : MonoBehaviour, INativeSigninDomain
    {
        private UniTaskCompletionSource<GoogleOAuthToken> _completionSource;
        private CancellationTokenRegistration _cancellationRegistration;


#if UNITY_IOS && !UNITY_EDITOR
        [DllImport( "__Internal" )]
        private static extern void GoogleAuth_RequestAccessToken(
            string clientId,
            string unityGameObjectName,
            string unityCallbackMethodName,
            string scope
        );

        [DllImport( "__Internal" )]
        private static extern void GoogleAuth_SignOut();
#endif
        private void Awake()
        {
            DontDestroyOnLoad( gameObject );
        }

        public UniTask<GoogleOAuthToken> RequestAccessTokenAsync( string clientId, string scope, CancellationToken cancellationToken )
        {
#if UNITY_IOS && !UNITY_EDITOR
            return RequestAccessTokenInternalAsync(clientId, scope, cancellationToken);
#else
            return UniTask.FromException<GoogleOAuthToken>(
                new PlatformNotSupportedException( "iOS Google Sign-In is only available on iOS device builds." )
            );
#endif
        }

        public void SignOut()
        {
#if UNITY_IOS && !UNITY_EDITOR
            GoogleAuth_SignOut();
#else
            Debug.LogWarning( "[GoogleOAuth iOS] SignOut is ignored outside iOS device build." );
#endif
        }

#if UNITY_IOS && !UNITY_EDITOR
        private UniTask<GoogleOAuthToken> RequestAccessTokenInternalAsync( string clientId, string scope, CancellationToken cancellationToken )
        {
            if( _completionSource != null )
            {
                throw new InvalidOperationException( "Google Sign-In request is already running." );
            }
            if( string.IsNullOrWhiteSpace( scope ) )
            {
                throw new ArgumentException( "scope is empty." );
            }

            _completionSource = new UniTaskCompletionSource<GoogleOAuthToken>();
            _cancellationRegistration  = cancellationToken.Register( () =>
            {
                _completionSource?.TrySetCanceled( cancellationToken );
                //_completionSource = null;
            } );

            GoogleAuth_RequestAccessToken( clientId, gameObject.name, nameof( OnNativeGoogleAuthResult ), scope );
            return _completionSource.Task;
        }
#endif
        public void OnNativeGoogleAuthResult( string json )
        {
            try
            {
                JSONNode root = JSON.Parse(json);

                bool success = root["success"].AsBool;

                if( !success )
                {
                    string error = root["error"].Value;
                    throw new Exception( $"Google Sign-In failed: {error}" );
                }

                string accessToken = root["accessToken"].Value;
                string tokenType = root["tokenType"].Value;
                string scope = root["scope"].Value;

                string expiresAtText = root["expiresAtUnixTime"].Value;
                if( !long.TryParse( expiresAtText, out long expiresAtUnixTime ) )
                {
                    expiresAtUnixTime =
                        DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 3600;
                }

                if( string.IsNullOrWhiteSpace( accessToken ) )
                    throw new Exception( "accessToken is empty." );

                var token = new GoogleOAuthToken
                {
                    accessToken = accessToken,
                    refreshToken = string.Empty,
                    expiresAtUnixTime = expiresAtUnixTime,
                    scope = scope,
                    tokenType = string.IsNullOrWhiteSpace(tokenType)
                        ? "Bearer"
                        : tokenType
                };

                _completionSource?.TrySetResult( token );
            }
            catch( Exception e )
            {
                _completionSource?.TrySetException( e );
            }
            finally
            {
                ClearPendingRequest();
            }
        }

        private void ClearPendingRequest()
        {
            _cancellationRegistration.Dispose();
            _completionSource = null;
        }

        private void OnDestroy()
        {
            if( _completionSource != null )
            {
                _completionSource.TrySetCanceled();
                ClearPendingRequest();
            }
        }
    }
}
