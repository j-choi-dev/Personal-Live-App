using Cysharp.Threading.Tasks;
using LiveApp.Util;
using LiveAppUI.Model;
using LiveAppUI.View;
using System;
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
        private IResourceListModel _resourceListModel;

        private NoticePurpose _noticePurpose;
        private ResourceListFailureData _pendingFailure;
        private bool _noticeActionRunning;


        [Inject]
        public void Initialize( IServerModalView loginView,
            IRoomModalView roomView,
            INoticeModalView noticeModalView,
            ILogInModel loginModel,
            IOAuthTokenModel authTokenModel,
            IResourceListModel resourceListModel )
        {
            _loginView = loginView;
            _roomView = roomView;
            _noticeModalView = noticeModalView;
            _loginModel = loginModel;
            _authTokenModel = authTokenModel;
            _resourceListModel = resourceListModel;
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
                .Subscribe(_ => HandleNoticeYesAsync().Forget(Debug.LogException))
                .AddTo(this);


            _noticeModalView.OnClickCancle
                .Merge(_noticeModalView.OnClickClose)
                .Subscribe(_ => HandleNoticeClose())
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

            _resourceListModel.OnLoadFailed
                .Subscribe(OnResourceLoadFailed)
                .AddTo(this);
        }

        private void TokenResultProcess(bool isResult)
        {
            Debug.Log($"Server Login Token Complete :: {isResult}");

            if (isResult)
            {
                if (_noticePurpose == NoticePurpose.InitialOAuth)
                {
                    _noticePurpose = NoticePurpose.None;
                    _pendingFailure = null;
                    _noticeModalView.SetActive(false);
                }

                _loginView.SetActive(true);
                _roomView.SetActive(false);
                return;
            }

            _noticePurpose = NoticePurpose.InitialOAuth;
            _pendingFailure = null;

            ShowNotice( "구글 인증 실패", "구글 인증에 실패했습니다.\n다시 시도하시겠습니까?", "YES : 재시도\nNO : 앱 종료");
        }

        private void OnResourceLoadFailed(ResourceListFailureData failure)
        {
            _pendingFailure = failure;

            string message;
            bool reauthorize;

            switch (failure.Kind)
            {
                case ResourceListFailureKind.MissingConfiguration:
                    message = "선택한 리소스와 서버에 해당하는 설정이 없습니다.";
                    reauthorize = false;
                    break;

                case ResourceListFailureKind.SheetOrTabNotFound:
                    message = "시트 ID와 탭 이름을 확인해주십시오.";
                    reauthorize = false;
                    break;

                case ResourceListFailureKind.Unauthorized:
                    message = "Google 토큰이 거절되었습니다.\n다시 인증하시겠습니까?";
                    reauthorize = true;
                    break;

                case ResourceListFailureKind.PermissionDenied:
                    message = "재 계정의 시트 공유 권한을 확인해 주세요.\nYES를 누르면 다시 인증한 뒤 조회합니다.";
                    reauthorize = true;
                    break;

                case ResourceListFailureKind.Network:
                    message = "네트워크 연결을 확인한 뒤 다시 시도해 주세요.";
                    reauthorize = false;
                    break;

                case ResourceListFailureKind.ConfigurationInitializationFailed:
                    message = "리소스 설정을 불러오지 못했습니다.\nYES를 누르면 설정 읽기부터 다시 시도합니다.";
                    reauthorize = false;
                    break;

                case ResourceListFailureKind.AuthorizationCancelled:
                    message = "Google 인증을 거절했거나 인증 창을 닫았습니다.\n다시 인증하시겠습니까?";
                    reauthorize = true;
                    break;

                default:
                    message = "리소스 조회에 실패했습니다. 설정과 로그를 확인해 주세요.";
                    reauthorize = false;
                    break;
            }

            _noticePurpose = reauthorize
                ? NoticePurpose.ResourceReauthorize
                : NoticePurpose.ResourceRetry;

            ShowNotice( "리소스 리스트 취득 실패", message, 
                reauthorize
                    ? "YES : 다시 인증 후 조회\nNO : 닫기"
                    : "YES : 다시 조회\nNO : 닫기");
        }

        private async UniTask HandleNoticeYesAsync()
        {
            if (_noticeActionRunning ||
                _resourceListModel.IsLoading || 
                _noticePurpose == NoticePurpose.None)
            {
                return;
            }

            var purpose = _noticePurpose;
            var failure = _pendingFailure;

            _noticePurpose = NoticePurpose.None;
            _pendingFailure = null;
            _noticeActionRunning = true;
            _noticeModalView.SetActive(false);

            try
            {
                switch (purpose)
                {
                    case NoticePurpose.InitialOAuth:
                        await _authTokenModel.InitilizeAuthProcess(
                            forceReauthorize: true);
                        break;

                    case NoticePurpose.ResourceRetry:
                        if (failure != null)
                        {
                            await _resourceListModel.GetResourceList(
                                failure.ResourceType,
                                failure.ServerType);
                        }
                        break;

                    case NoticePurpose.ResourceReauthorize:
                        if (failure != null)
                        {
                            await _resourceListModel.ReauthorizeAndGetResourceList(
                                failure.ResourceType,
                                failure.ServerType);
                        }
                        break;
                }
            }
            catch (OperationCanceledException)
            {
                // 취소된 작업을 자동으로 다시 실행하지 않습니다.
            }
            catch (Exception e)
            {
                Debug.LogException(e);

                _noticePurpose = purpose;
                _pendingFailure = failure;

                ShowNotice( "처리 실패", "요청을 완료하지 못했습니다. 다시 시도하시겠습니까?",
                    purpose == NoticePurpose.InitialOAuth
                        ? "YES : 재시도\nNO : 앱 종료"
                        : "YES : 재시도\nNO : 닫기");
            }
            finally
            {
                _noticeActionRunning = false;
            }
        }

        private void HandleNoticeClose()
        {
            if (_noticeActionRunning)
            {
                return;
            }

            var exitApplication = _noticePurpose == NoticePurpose.InitialOAuth;

            _noticePurpose = NoticePurpose.None;
            _pendingFailure = null;
            _noticeModalView.SetActive(false);

            if (exitApplication)
            {
                Application.Quit(1);
            }
        }

        private void ShowNotice(
            string title,
            string message,
            string subMessage)
        {
            _noticeModalView.SetHeaderTitle(title);
            _noticeModalView.SetMainMessage(message);
            _noticeModalView.SetSubMessage(subMessage);
            _noticeModalView.SetActive(true);
        }
    }
}
