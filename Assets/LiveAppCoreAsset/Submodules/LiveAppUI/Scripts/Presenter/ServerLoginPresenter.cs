using Cysharp.Threading.Tasks;
using LiveAppUI.Model;
using UniRx;
using UnityEngine;
using Zenject;

namespace LiveAppUI.Presenter
{
    /// <summary>
    /// 서버 로그인 관련 View-Model의 Presenter
    /// </summary>
    public class ServerLoginPresenter : MonoBehaviour
    {
        private IServerModalView _loginView;
        private IRoomModalView _roomView;

        private ILogInModel _loginModel;
        private IOAuthTokenModel _authTokenModel;


        [Inject]
        public void Initialize( IServerModalView loginView,
            IRoomModalView roomView,
            ILogInModel loginModel,
            IOAuthTokenModel authTokenModel )
        {
            _loginView = loginView;
            _roomView = roomView;
            _loginModel = loginModel;
            _authTokenModel = authTokenModel;
        }

        private void Awake()
        {
            Debug.LogError(
                $"[GoogleAuth:C#0] ServerLoginPresenter.Awake. " +
                $"object={gameObject.name}, " +
                $"active={gameObject.activeInHierarchy}, " +
                $"enabled={enabled}"
            );
            InitView();
            Debug.LogError( "[GoogleAuth:C#0.2] InitView completed." );
            InitModel();
            Debug.LogError( "[GoogleAuth:C#0.3] InitModel completed." );
        }

        private async void Start()
        {
            Debug.LogError(
                "[GoogleAuth:C#1] ServerLoginPresenter.Start entered."
            );
            _loginView.SetActive( false );
            _roomView.SetActive( false );

            Debug.LogError(
                "[GoogleAuth:C#1] InitializeAuthProcess calling."
            );
            var result = await _authTokenModel.InitilizeAuthProcess();

            Debug.LogError(
                $"[GoogleAuth:C#1] InitializeAuthProcess result={result}"
            );
            if( result == false )
            {
                return;
            }
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
                .Merge( _roomView.OnClose )
                .Subscribe( _ =>
                {
                    _roomView.SetActive( false );
                    _loginView.SetActive( true );
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

            _loginModel.OnRoomEnterSuccess
                .Subscribe( _ =>
                {
                    _roomView.SetActive( false );
                    _loginView.SetActive( false );
                } )
                .AddTo( this );

            _authTokenModel.OnCompleteTokenProcess
                .Subscribe( isResullt => TokenResultProcess(isResullt) )
                .AddTo( this );
        }

        private void TokenResultProcess(bool isResult)
        {
            Debug.Log( $"Token Complete :: {isResult}" );
            if( isResult )
            {
                _loginView.SetActive( true );
                _roomView.SetActive( false );
            }
            else
            {
                Debug.Log( $"Application Quit" );
                Application.Quit( 1 );
            }
        }
    }
}
