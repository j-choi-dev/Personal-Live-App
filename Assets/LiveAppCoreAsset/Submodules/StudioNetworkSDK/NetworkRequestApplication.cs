using Cysharp.Threading.Tasks;
using StudioNetworkSDK.Domain;
using StudioNetworkSDK.Infrastructure;
using System;
using UniRx;
using UnityEngine;

namespace StudioNetworkSDK.Application
{
    public class NetworkRequestApplication : INetworkRequestApplication
    {
        private IServerConfigDomain _serverConfigDomain;
        private ISenderDomain _sender;
        private IReceiverDomain _receiver; 
        
        private CompositeDisposable _disposables = new CompositeDisposable();

        private readonly Subject<bool> _onLoginResult = new Subject<bool>();
        public IObservable<bool> OnLoginResult => _onLoginResult;

        public NetworkRequestApplication( IServerConfigDomain serverConfigDomain,
            ISenderDomain sender,
            IReceiverDomain receiver )
        {
            _serverConfigDomain = serverConfigDomain;
            _sender = sender;
            _receiver = receiver;

            _receiver.OnMessageReceived
                .ObserveOnMainThread() 
                .Subscribe( HandleMessageReceived )
                .AddTo( _disposables );
        }

        public async UniTask<bool> Initialize()
        {
            var config = _serverConfigDomain.GetConfigData();
            var initResult = await _sender.Initialize( config );

            if( initResult )
            {
                var client = ((MqTTSender)_sender).GetClient();
                _receiver.Initialize( client, config );
            }

            return initResult;
        }

        public UniTask<bool> SendLoginRequest( string id, string pw )
        {
            return _sender.SendLoginRequest( id, pw );
        }

        public UniTask<bool> SendLoginRequest_Test( string id, string pw )
        {
            return _sender.SendLoginRequest( "testuser", "1234" );
        }
        private void HandleMessageReceived( MqttMessageData msgData )
        {
            NetworkProtocol protocol = ParseProtocol(msgData);

            switch( protocol )
            {
                case NetworkProtocol.LoginResponse:
                    ProcessLoginResponse( msgData.Payload );
                    break;

                case NetworkProtocol.Unknown:
                default:
                    Debug.LogWarning( $"[App Layer] 매칭되는 프로토콜이 없어 무시됨. Topic: {msgData.Topic}" );
                    break;
            }
        }

        private NetworkProtocol ParseProtocol( MqttMessageData msgData )
        {
            // 토픽 규칙이 명확하다면 Switch문이나 Dictionary로 매핑하는 것이 더 빠르고 깔끔해.
            if( msgData.Topic.Contains( "login/response" ) )
            {
                return NetworkProtocol.LoginResponse;
            }

            return NetworkProtocol.Unknown;
        }

        private void ProcessLoginResponse( string payload )
        {
            try
            {
                // JSON 문자열을 DTO 객체로 변환
                LoginResponse res = JsonUtility.FromJson<LoginResponse>(payload);

                bool isSuccess = (res.status == "success");

                if( isSuccess )
                {
                    Debug.Log( "[App Layer] 로그인 데이터 파싱 결과: 성공" );
                }
                else
                {
                    Debug.LogError( $"[App Layer] 로그인 데이터 파싱 결과: 실패 사유 - {res.message}" );
                }

                _onLoginResult.OnNext( isSuccess );
            }
            catch( Exception ex )
            {
                Debug.LogError( $"[App Layer] 로그인 페이로드 파싱 중 에러 발생: {ex.Message}" );
                _onLoginResult.OnNext( false );
            }
        }

        public void Dispose()
        {
            _disposables.Dispose();
            _onLoginResult.OnCompleted();
            _onLoginResult.Dispose();
        }
    }
}
