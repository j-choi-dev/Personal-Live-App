using Cysharp.Threading.Tasks;
using StudioNetworkSDK.Application;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace LiveAppUI.Model
{
    public class LogInModel : ILogInModel, IDisposable
    {
        private INetworkRequestApplication _networkApplication;
        private CompositeDisposable _disposable = new CompositeDisposable();
        public IReadOnlyList<string> ServerList => Enum.GetNames( typeof( ServerItem ) );
        public IReadOnlyList<string> RoomList => new List<string>() { "Room_001", "Room_002", "Room_003" }; // TODO 임시 데이터 @Choi 26.05.01

        private Subject<bool> _onLoginSuccess = new Subject<bool>();
        public IObservable<bool> OnLoginSuccess => _onLoginSuccess;


        private Subject<bool> _onRoomEnterSuccess  = new Subject<bool>();
        public IObservable<bool> OnRoomEnterSuccess => _onRoomEnterSuccess;

        public LogInModel( INetworkRequestApplication networkApplication )
        {
            _networkApplication = networkApplication;

            _networkApplication.OnLoginResult
                .Subscribe( _ => _onLoginSuccess.OnNext( _ ) )
                .AddTo( _disposable );
        }

        public async UniTask<bool> Initialize()
            => await _networkApplication.Initialize();

        public async UniTask LoginProcess( string id, string pw, ServerItem item )
        {
            await _networkApplication.SendLoginRequest_Test( id, pw );
        }

        public async UniTask RoomEnterProcess( int index, string name )
        {
            await UniTask.WaitForSeconds( 0.25f );
            _onRoomEnterSuccess.OnNext( true );
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

    }
}
