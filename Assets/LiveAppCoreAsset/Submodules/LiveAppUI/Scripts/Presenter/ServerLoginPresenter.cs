using Cysharp.Threading.Tasks;
using LiveAppUI.Model;
using UniRx;
using UnityEngine;
using Zenject;

namespace LiveAppUI.Presenter
{
    public class ServerLoginPresenter : MonoBehaviour
    {
        private IServerModalView _loginView;
        private IRoomModalView _roomView;

        private ILogInModel _loginModel;


        [Inject]
        public void Initialize( IServerModalView loginView,
            IRoomModalView roomView,
            ILogInModel loginModel )
        {
            _loginView = loginView;
            _roomView = roomView;
            _loginModel = loginModel;
        }

        private async void Awake()
        {
            InitView();
            InitModel();
        }

        private void Start()
        {
            _loginView.SetActive( true );
            _roomView.SetActive( false );
        }

        private void InitView()
        {
            _loginView.OnClose
                .Subscribe( x =>
                {
                    _loginView.SetActive( false );
                    Application.Quit();
                } )
                .AddTo( this );
            _loginView.OnClicLogin
                .Subscribe( x => _loginModel.LoginProcess( _loginView.CurrentID, 
                    _loginView.CurrentPassword, 
                    ( ServerItem )( _loginView.CurrentIndex+1 ) ).Forget()
                )
                .AddTo( this );

            _roomView.OnClickEnter
                .Subscribe( _ => _loginModel.RoomEnterProcess( _roomView.CurrenIndex,
                    _roomView.CurrentName ) )
                .AddTo( this );

            _roomView.OnClickExit
                .Merge(_roomView.OnClose )
                .Subscribe( _ =>
                {
                    _roomView.SetActive( false );
                    _loginView.SetActive ( true );
                } )
                .AddTo( this );

            _loginView.SetServerList( _loginModel.ServerList );
            _roomView.SetRoomList( _loginModel.RoomList );
        }

        private void InitModel()
        {
            _loginModel.OnLoginSuccess
                .Subscribe( _ =>
                {
                    _roomView.SetActive( true );
                    _loginView.SetActive( false );
                } )
                .AddTo( this );

            _loginModel.OnRoomEnterSuccess
                .Subscribe( _ =>
                {
                    _roomView.SetActive( false );
                    _loginView.SetActive( false );
                } )
                .AddTo( this );
        }
    }
}
