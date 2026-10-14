using Cysharp.Threading.Tasks;
using LiveApp.Util;
using LiveAppUI.Model;
using LiveAppUI.View;
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
        private INoticeModalView _noticeModalView;

        private ILogInModel _loginModel;
        private IOAuthTokenModel _authTokenModel;
        private enum NoticePurpose
        {
            None,
            InitialOAuth,
            ResourceRetry,
            ResourceReauthorize
        }

        private NoticePurpose _noticePurpose;
        private ResourceListFailureData _pendingFailure;
        private bool _noticeActionRunning;


        [Inject]
        public void Initialize( IServerModalView loginView,
            IRoomModalView roomView,
            INoticeModalView noticeModalView,
            ILogInModel loginModel,
            IOAuthTokenModel authTokenModel)
        {
            _loginView = loginView;
            _roomView = roomView;
            _noticeModalView = noticeModalView;
            _loginModel = loginModel;
            _authTokenModel = authTokenModel;
        }

        private void Awake()
        {
            InitView();
            InitModel();
        }

        private async void Start()
        {
            _noticeModalView.SetActive(false);
            // TODO Infra층에 가져가야함.(ObsAgentController의 NonLazy,  new ReplaySubject<string>( 1 ) 참고) Refactor 대상 @Choi 26.09.01
            var id = PlayerPrefsUtil.GetStringValueByKey("Server.ID");
            if( string.IsNullOrWhiteSpace( id ) == false )
            {
                _loginView.SetServerIdWithoutNotify( id );
            }
            var pw = PlayerPrefsUtil.GetStringValueByKey("Server.PW");
            if( string.IsNullOrWhiteSpace( pw ) == false )
            {
                _loginView.SetServerPasswordWithoutNotify( pw );
            }
            var roomName = PlayerPrefsUtil.GetStringValueByKey("Room.Name");
            if( string.IsNullOrWhiteSpace( roomName ) == false )
            {
                _roomView.SetNameWithoutNotify( roomName );
            }

            _loginView.SetActive( false );
            _roomView.SetActive( false );
            var result = await _authTokenModel.InitilizeAuthProcess();

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
                .Subscribe( x =>
                {
                    _loginModel.LoginProcess( _loginView.CurrentID,
                    _loginView.CurrentPassword,
                    ( ServerItem )( _loginView.CurrentIndex+1 ) ).Forget();
                    // TODO Infra층에 가져가야함.(ObsAgentController의 NonLazy,  new ReplaySubject<string>( 1 ) 참고) Refactor 대상 @Choi 26.09.01
                    PlayerPrefsUtil.SetStringValueByKey( "Server.ID", _loginView.CurrentID );
                    PlayerPrefsUtil.SetStringValueByKey( "Server.PW", _loginView.CurrentPassword );
                } )
                .AddTo( this );

            _roomView.OnClickEnter
                .Subscribe( _ =>
                {
                    _loginModel.RoomEnterProcess( _roomView.CurrenIndex, _roomView.CurrentName );
                    // TODO Infra층에 가져가야함.(ObsAgentController의 NonLazy,  new ReplaySubject<string>( 1 ) 참고) Refactor 대상 @Choi 26.09.01
                    PlayerPrefsUtil.SetStringValueByKey( "Room.Name", _roomView.CurrentName );
                } )
                .AddTo( this );

            _roomView.OnClickExit
                .Merge( _roomView.OnClose )
                .Subscribe( _ =>
                {
                    _roomView.SetActive( false );
                    _loginView.SetActive( true );
                } )
                .AddTo( this );

            _noticeModalView.OnClickYes
                .Subscribe(async x => 
                {
                    _noticeModalView.SetActive(false);
                    var result = await _authTokenModel.InitilizeAuthProcess(forceReauthorize: true);

                    if (result == false)
                    {
                        return;
                    }
                })
                .AddTo(this);


            _noticeModalView.OnClickCancle
                .Merge(_noticeModalView.OnClickClose)
                .Subscribe( x => Application.Quit(1))
                .AddTo(this);

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
                .Subscribe( isResullt => TokenResultProcess( isResullt ) )
                .AddTo( this );
        }

        private void TokenResultProcess( bool isResult )
        {
            Debug.Log( $"Server Login Token Complete :: {isResult}" );
            if( isResult )
            {
                _loginView.SetActive( true );
                _roomView.SetActive( false );
            }
            else
            {
                _noticeModalView.SetActive(true);
                _noticeModalView.SetHeaderTitle("구글 인증 실패");
                _noticeModalView.SetMainMessage("구글 인증에 실패했습니다.\n다시 시도하겠습니까?");
                _noticeModalView.SetSubMessage("YES : 재시도\nNO : 앱 종료");
                //Debug.Log( $"Application Quit" );
                //Application.Quit( 1 );
            }
        }
    }
}
