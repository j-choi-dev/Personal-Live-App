using Cysharp.Threading.Tasks;
using LiveAppUI.Model;
using StudioRendererSDK.Domain;
using System.Threading;
using UnityEngine;
using Zenject;
using UniRx;

namespace LiveAppUI.Presenter
{
    public class YoutubeConfigPresenter : MonoBehaviour
    {
        private IYoutubeConfigView _youtubeConfigView;
        private IObsAgentModel _obsAgentModel;

        private CancellationTokenSource _pollCancellation;

        [Inject]
        public void Construct( IYoutubeConfigView youtubeConfigView,
            IObsAgentModel obsAgentModel )
        {
            _youtubeConfigView = youtubeConfigView;
            _obsAgentModel = obsAgentModel;
        }
        private void Awake()
        {
            _youtubeConfigView.OnPrepareButton
                .Subscribe( _ => PrepareProcessAsync().Forget() )
                .AddTo( this );
            _youtubeConfigView.OnStartButton
                .Subscribe( _ => StartProcessAsync().Forget() )
                .AddTo( this );
            _youtubeConfigView.OnStopButton
                .Subscribe( _ => StopProcessAsync().Forget() )
                .AddTo( this );
        }

        private async UniTask<bool> PrepareProcessAsync()
        {
            string title = _youtubeConfigView.Title;
            string streamKey = _youtubeConfigView.StreamKey;
            if( string.IsNullOrWhiteSpace( title ) )
            {
                _youtubeConfigView.SetFailed( "방송 제목을 입력하세요." );
                return false;
            }

            if( string.IsNullOrWhiteSpace( streamKey ) )
            {
                _youtubeConfigView.SetFailed( "YouTube Stream Key를 입력하세요." );
                return false;
            }

            _youtubeConfigView.GetResolution( out int width, out int height );

            var request = new YoutubeLivePrepareRequest
            {
                title = title,
                width = width,
                height = height,
                streamKey = streamKey
            };

            _youtubeConfigView.SetPreparing( "방송 준비 요청 중..." );

            bool accepted = await _obsAgentModel.PrepareYoutubeLiveProcess( request );

            if( !accepted )
            {
                _youtubeConfigView.SetFailed( "Agent가 방송 준비 요청을 거부했습니다." );
                return false;
            }

            StartPolling();
            return true;
        }

        private async UniTask<bool> StartProcessAsync()
        {
            _youtubeConfigView.SetStarting( "방송 시작 요청 중..." );

            bool accepted =                await _obsAgentModel                    .StartYoutubeLiveProcess();

            if( !accepted )
            {
                _youtubeConfigView.SetReady( "방송 시작 요청에 실패했습니다." );
                return false;
            }

            StartPolling();
            return true;
        }

        private async UniTask<bool> StopProcessAsync()
        {
            _youtubeConfigView.SetStarting( "방송 종료 요청 중..." );
            bool accepted =                await _obsAgentModel                    .StopYoutubeLiveProcess();
            if( !accepted )
            {
                _youtubeConfigView.SetFailed( "방송 종료 요청에 실패했습니다." );
                return false;
            }

            StartPolling();
            return true;
        }

        private void StartPolling()
        {
            _pollCancellation?.Cancel();
            _pollCancellation?.Dispose();

            _pollCancellation = CancellationTokenSource.CreateLinkedTokenSource( this.GetCancellationTokenOnDestroy() );

            PollStatusAsync( _pollCancellation.Token ).Forget();
        }

        private async UniTaskVoid PollStatusAsync( CancellationToken cancellationToken )
        {
            while( cancellationToken.IsCancellationRequested == false )
            {
                YoutubeLiveStatusResponse status = await _obsAgentModel.GetYoutubeLiveStatusProcess();

                if( status != null )
                {
                    ApplyStatus( status );

                    if( status.state == "READY" ||
                        status.state == "LIVE" ||
                        status.state == "FAILED" ||
                        status.state == "IDLE" )
                    {
                        return;
                    }
                }

                await UniTask.Delay( 1000, cancellationToken: cancellationToken );
            }
        }

        private void ApplyStatus( YoutubeLiveStatusResponse status )
        {
            switch( status.state )
            {
                case "IDLE":
                    _youtubeConfigView.SetIdleStatus();
                    break;

                case "PREPARING":
                    _youtubeConfigView.SetPreparing( status.message );
                    break;

                case "READY":
                    _youtubeConfigView.SetReady( status.message );
                    break;

                case "STARTING":
                    _youtubeConfigView.SetStarting( status.message );
                    break;

                case "LIVE":
                    //_youtubeConfigView.SetLive( status.message );
                    break;

                case "STOPPING":
                    _youtubeConfigView.SetStarting( status.message );
                    break;

                case "FAILED":
                    _youtubeConfigView.SetFailed( status.message );
                    break;
            }
        }

        private void OnDestroy()
        {
            _pollCancellation?.Cancel();
            _pollCancellation?.Dispose();
            _pollCancellation = null;
        }
    }
}
