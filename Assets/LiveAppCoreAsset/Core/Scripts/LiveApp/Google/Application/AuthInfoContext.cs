using Cysharp.Threading.Tasks;
using LiveAppCore.Google.Domain;
using StudioSystemSDK.Domain;
using System;
using System.IO;
using UniRx;
using UnityEngine;

namespace LiveAppCore.Google.Application
{
    public class AuthInfoContext : IAuthInfoContext
    {
        private IFileSystemDomain _fileSystemDomain;
        private IFileSerializeDomain _fileSerializeDomain;
        private IGoogleAuthInfoStorage _googleAuthInfoStorage;
        private IGoogleAuthTokenDomain _googleAuthDomain;

        public AuthInfoContext( IFileSystemDomain fileSystemDomain,
            IFileSerializeDomain fileSerializeDomain,
            IGoogleAuthInfoStorage googleAuthInfoStorage,
            IGoogleAuthTokenDomain googleAuthDomain )
        {
            _fileSystemDomain = fileSystemDomain;
            _fileSerializeDomain = fileSerializeDomain;
            _googleAuthInfoStorage = googleAuthInfoStorage;
            _googleAuthDomain = googleAuthDomain;
        }

        private Subject<bool> _onCompleteTokenProcess = new Subject<bool>();
        public IObservable<bool> OnCompleteTokenProcess => _onCompleteTokenProcess;

        public string Token => _googleAuthDomain.Token;

        public async UniTask<bool> InitilizeAuthProcess()
        {
            try
            {
                var authInfoPath = Path.Combine(UnityEngine.Application.streamingAssetsPath, OAuthConstValue.BinFileName);
                if( _fileSystemDomain.IsFileExist( authInfoPath ) == false )
                {
                    throw new FileNotFoundException( "File Not Exist :: ", authInfoPath );
                }
                var rawData = await _fileSystemDomain.LoadTextFile( authInfoPath );
                var oauthSettings = _fileSerializeDomain.DeserializeFromJson<GoogleOAuthSettings>( rawData );
                _googleAuthInfoStorage.SetOAuthSettings( oauthSettings );
                _googleAuthDomain.SetAuthValue( oauthSettings );
                var token = await _googleAuthDomain.GetAccessTokenAsync();
                _googleAuthInfoStorage.SetOAuthToken( token );

                _onCompleteTokenProcess.OnNext( true );
                return true;
            }
            catch( Exception e )
            {
                Debug.LogException( e );
                _onCompleteTokenProcess.OnNext( false );
                return false;
            }
        }
    }
}
