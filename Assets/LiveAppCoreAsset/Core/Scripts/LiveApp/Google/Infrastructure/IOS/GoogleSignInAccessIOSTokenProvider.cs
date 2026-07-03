#if UNITY_IOS || !UNITY_EDITOR

using System;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using LiveAppCore.Google.Domain;
using SimpleJSON;
using UnityEngine;

namespace LiveAppCore.Google.Infrastructure
{
    // TODO 일단 보존 @Choi 26.07.02
    public sealed class GoogleSignInAccessIOSTokenProvider : MonoBehaviour
    {
        private static GoogleSignInAccessIOSTokenProvider _instance;

        private UniTaskCompletionSource<string> _tokenCompletionSource;

        private string _iosClientId;
        private string _scope;

        [DllImport( "__Internal" )]
        private static extern void GoogleAuth_Configure(
            string clientId,
            string scope
        );

        [DllImport( "__Internal" )]
        private static extern void GoogleAuth_RequestAccessToken(
            string unityGameObjectName,
            string unityCallbackMethodName
        );

        [DllImport( "__Internal" )]
        private static extern void GoogleAuth_SignOut();

        public static GoogleSignInAccessIOSTokenProvider Create(
            string iosClientId,
            string scope
        )
        {
            if( _instance != null )
                return _instance;

            GameObject obj = new GameObject(nameof(GoogleSignInAccessIOSTokenProvider));
            DontDestroyOnLoad( obj );

            _instance = obj.AddComponent<GoogleSignInAccessIOSTokenProvider>();
            _instance.Initialize( iosClientId, scope );

            return _instance;
        }

        private void Initialize(
            string iosClientId,
            string scope
        )
        {
            _iosClientId = iosClientId;
            _scope = scope;

            GoogleAuth_Configure( _iosClientId, _scope );
        }

        public UniTask<string> GetAccessTokenAsync(
            CancellationToken cancellationToken = default
        )
        {
            _tokenCompletionSource = new UniTaskCompletionSource<string>();

            GoogleAuth_RequestAccessToken(
                gameObject.name,
                nameof( OnNativeAccessTokenResult )
            );

            cancellationToken.Register( () =>
            {
                _tokenCompletionSource?.TrySetCanceled( cancellationToken );
            } );

            return _tokenCompletionSource.Task;
        }

        public UniTask ClearAsync()
        {
            GoogleAuth_SignOut();
            return UniTask.CompletedTask;
        }

        // Native에서 UnitySendMessage로 호출.
        private void OnNativeAccessTokenResult( string json )
        {
            try
            {
                JSONNode root = JSON.Parse(json);

                bool success = root["success"].AsBool;

                if( !success )
                {
                    string error = root["error"].Value;
                    _tokenCompletionSource?.TrySetException(
                        new Exception( $"Google Sign-In failed: {error}" )
                    );
                    return;
                }

                string accessToken = root["accessToken"].Value;

                if( string.IsNullOrEmpty( accessToken ) )
                {
                    _tokenCompletionSource?.TrySetException(
                        new Exception( "Access token is empty." )
                    );
                    return;
                }

                _tokenCompletionSource?.TrySetResult( accessToken );
            }
            catch( Exception e )
            {
                _tokenCompletionSource?.TrySetException( e );
            }
        }
    }
}

#endif