using Cysharp.Threading.Tasks;
using LiveAppCore.Google.Domain;
using StudioSystemSDK.Domain;
using System;
using System.IO;
using UniRx;
using UnityEngine;

namespace LiveAppCore.Google.Application
{
    /// <summary>
    /// Auth 정보 처리 관련 Application층 구현 클래스
    /// </summary>
    public class AuthInfoContext : IAuthInfoContext
    {
        private IFileSystemDomain _fileSystemDomain;
        private IFileSerializeDomain _fileSerializeDomain;
        private IGoogleAuthInfoStorage _googleAuthInfoStorage;
        private IGoogleAuthTokenDomain _googleAuthDomain;
        private ICryptoKeySettingDomain _cryptoKeySetting;
        private ICryptoProcessDomain _cryptoDomain;

        public AuthInfoContext( IFileSystemDomain fileSystemDomain,
            IFileSerializeDomain fileSerializeDomain,
            IGoogleAuthInfoStorage googleAuthInfoStorage,
            IGoogleAuthTokenDomain googleAuthDomain,
            ICryptoKeySettingDomain cryptoKeySettingDomain,
            ICryptoProcessDomain cryptoDomain )
        {
            _fileSystemDomain = fileSystemDomain;
            _fileSerializeDomain = fileSerializeDomain;
            _googleAuthInfoStorage = googleAuthInfoStorage;
            _googleAuthDomain = googleAuthDomain;
            _cryptoKeySetting = cryptoKeySettingDomain;
            _cryptoDomain = cryptoDomain;
        }

        private Subject<bool> _onCompleteTokenProcess = new Subject<bool>();
        public IObservable<bool> OnCompleteTokenProcess => _onCompleteTokenProcess;

        public string Token => _googleAuthDomain.Token;

        public async UniTask<bool> InitilizeAuthProcess()
        {
            Debug.LogWarning( "[GoogleAuth:C#2] InitilizeAuthProcess entered." );

            try
            {
                var authInfoPath = Path.Combine(UnityEngine.Application.streamingAssetsPath, OAuthConstValue.BinFileName);
                if( _fileSystemDomain.IsFileExist( authInfoPath ) == false )
                {
                    throw new FileNotFoundException( "File Not Exist :: ", authInfoPath );
                }

                Debug.LogWarning( $"[GoogleAuth:C#2] authInfoPath={authInfoPath}" );

                bool exists = _fileSystemDomain.IsFileExist(authInfoPath);

                Debug.LogWarning( $"[GoogleAuth:C#2] auth.bin exists={exists}" );

                if( !exists )
                {
                    throw new FileNotFoundException( "Google OAuth auth.bin does not exist.", authInfoPath );
                }

                Debug.LogWarning( "[GoogleAuth:C#2] Loading auth.bin." );
                var rawData = await _fileSystemDomain.LoadTextFile( authInfoPath );


                Debug.LogWarning( $"[GoogleAuth:C#2] auth.bin loaded. length={rawData?.Length ?? 0}" );
                var decryptedText = _cryptoDomain.ConvertDecryptedString( rawData, _cryptoKeySetting.CryptoKey );

                Debug.LogWarning( $"[GoogleAuth:C#2] Decrypted. length={decryptedText?.Length ?? 0}" );
                var oauthSettings = _fileSerializeDomain.DeserializeFromJson<GoogleOAuthSettings>( decryptedText );

                Debug.LogWarning( $"[GoogleAuth:C#2] Settings parsed. iosClientIdExists={!string.IsNullOrWhiteSpace( oauthSettings.IOSClientId )}, scopeExists={!string.IsNullOrWhiteSpace( oauthSettings.SheetsReadonlyScope )}" );
                _googleAuthInfoStorage.SetOAuthSettings( oauthSettings );
                _googleAuthDomain.SetAuthValue( oauthSettings );
                Debug.LogWarning( "[GoogleAuth:C#2] GetAccessTokenAsync calling." );
                var token = await _googleAuthDomain.GetAccessTokenAsync();
                Debug.LogWarning( $"[GoogleAuth:C#2] Token received. hasToken={!string.IsNullOrWhiteSpace( token )}");
                _googleAuthInfoStorage.SetOAuthToken( token );
                _onCompleteTokenProcess.OnNext( true );
                return true;
            }
            catch( Exception e )
            {
                Debug.LogError( $"[GoogleAuth:C#2] Auth failed. {e.GetType().Name}: {e.Message}" );
                Debug.LogException( e );
                _onCompleteTokenProcess.OnNext( false );
                return false;
            }
        }
    }
}
