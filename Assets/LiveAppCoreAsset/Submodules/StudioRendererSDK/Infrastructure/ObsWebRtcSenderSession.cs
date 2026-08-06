using Cysharp.Threading.Tasks;
using StudioRendererSDK.Domain;
using System;
using System.Text;
using System.Threading;
using UniRx;
using Unity.WebRTC;
using UnityEngine;
using UnityEngine.Networking;

namespace StudioRendererSDK.Infrastructure
{
    // @Use
    [DisallowMultipleComponent]
    public sealed class ObsWebRtcSenderSession : MonoBehaviour, IWebRtcSenderSessionDomain
    {
        [Header("Required Reference")]
        [SerializeField]
        private ObsWebRtcVideoTrackProvider videoTrackProvider;

        [Header("Manual Test Connection")]
        //[SerializeField] private string agentEndpoint = "http://192.168.0.10:7443";
        //[SerializeField] private string agentToken = string.Empty;
        [SerializeField] private string sessionId = "iphone-main";

        [Header("Video")]
        [SerializeField] private int bitrate = 5_000_000;
        [SerializeField] private int frameRate = 30;

        [Header("Timeout")] 
        [SerializeField] private int requestTimeoutSeconds = 10;
        [SerializeField] private int iceGatheringTimeoutSeconds = 10;
        [SerializeField] private int answerTimeoutSeconds = 20;
        [SerializeField] private int connectionTimeoutSeconds = 15;
        [SerializeField] private int answerPollingMilliseconds = 250;

        private RTCPeerConnection _peerConnection;
        private CancellationTokenSource _sessionCancellation;

        private bool _isStarting;

        public bool IsConnected => _peerConnection != null && _peerConnection.ConnectionState == RTCPeerConnectionState.Connected;

        private Subject<string> _onMessageChanged = new Subject<string>();
        public IObservable<string> OnMessageChanged => _onMessageChanged;

        private Subject<bool> _onConnectionChanged = new Subject<bool>();
        public IObservable<bool> OnConnectionChanged => _onConnectionChanged;

        public async UniTask<bool> StartVideoLinkAsync( string endpoint, string token )
        {
            if( _isStarting )
            {
                PublishStatus( "WebRTC 연결 작업이 이미 진행 중입니다." );
                return false;
            }

            _isStarting = true;
            StopVideoLink();
            _sessionCancellation = new CancellationTokenSource();
            CancellationToken cancellationToken = _sessionCancellation.Token;

            try
            {
                ValidateConfiguration( endpoint, token );
                string normalizedEndpoint = NormalizeEndpoint(endpoint);
                PublishStatus( "WebRTC 세션 초기화 요청 중" );
                await ResetSessionAsync( normalizedEndpoint, token, cancellationToken );
                CreatePeerConnection();
                videoTrackProvider.CreateTrack( _peerConnection, bitrate, frameRate );
                PublishStatus( "WebRTC SDP Offer 생성 중" );
                RTCSessionDescriptionAsyncOperation createOfferOperation = _peerConnection.CreateOffer();
                await WaitOperationAsync( createOfferOperation, cancellationToken );
                RTCSessionDescription offer = createOfferOperation.Desc;

                RTCSetSessionDescriptionAsyncOperation setLocalOperation = _peerConnection .SetLocalDescription( ref offer);
                await WaitOperationAsync( setLocalOperation, cancellationToken );

                PublishStatus( "WebRTC ICE Candidate 수집 중" );

                await WaitForIceGatheringAsync( cancellationToken );

                RTCSessionDescription? localDescription = _peerConnection.LocalDescription;

                if( !localDescription.HasValue )
                {
                    throw new InvalidOperationException( "Local SDP Offer가 없습니다." );
                }

                await PostOfferAsync( normalizedEndpoint, token, localDescription.Value.sdp, cancellationToken );

                PublishStatus( "PC Browser Source의 Answer 대기 중" );

                VideoSessionDescriptionResponse answerResponse = await WaitForAnswerAsync( normalizedEndpoint, token, cancellationToken);

                var answer = new RTCSessionDescription { type = RTCSdpType.Answer, sdp = answerResponse.sdp };

                RTCSetSessionDescriptionAsyncOperation setRemoteOperation = _peerConnection.SetRemoteDescription( ref answer);

                await WaitOperationAsync( setRemoteOperation, cancellationToken );

                PublishStatus( "WebRTC Peer 연결 완료 대기 중" );
                await WaitForConnectionAsync( cancellationToken );

                PublishStatus( "iPhone 영상이 PC로 전송되고 있습니다." );
                _onConnectionChanged.OnNext( true );

                return true;
            }
            catch( OperationCanceledException )
            {
                PublishStatus( "WebRTC 연결 작업이 취소되었습니다." );
                DisposePeerConnection();
                return false;
            }
            catch( Exception exception )
            {
                PublishStatus( $"WebRTC 연결 실패\n{exception.Message}" );
                Debug.LogException( exception, this );
                DisposePeerConnection();
                _onConnectionChanged.OnNext( false );
                return false;
            }
            finally
            {
                _isStarting = false;
            }
        }

        public void StopVideoLink()
        {
            if( _sessionCancellation != null )
            {
                _sessionCancellation.Cancel();
                _sessionCancellation.Dispose();
                _sessionCancellation = null;
            }
            DisposePeerConnection();
            _onConnectionChanged.OnNext( false );
            PublishStatus( "WebRTC 영상 연결이 중지되었습니다." );
        }

        private void CreatePeerConnection()
        {
            DisposePeerConnection();

            RTCConfiguration configuration = default;

            // 최초 LAN 테스트에서는 별도 STUN/TURN을
            // 사용하지 않는다.
            configuration.iceServers = Array.Empty<RTCIceServer>();
            _peerConnection = new RTCPeerConnection( ref configuration );
            _peerConnection .OnConnectionStateChange = OnConnectionStateChanged;
            _peerConnection .OnIceConnectionChange = OnIceConnectionStateChanged;
        }

        private void OnConnectionStateChanged( RTCPeerConnectionState state )
        {
            PublishStatus( $"WebRTC ConnectionState: {state}" );

            if( state == RTCPeerConnectionState.Connected )
            {
                _onConnectionChanged.OnNext( true );
            }
            else if( state == RTCPeerConnectionState.Failed ||
                     state == RTCPeerConnectionState.Closed ||
                     state == RTCPeerConnectionState.Disconnected )
            {
                _onConnectionChanged.OnNext( false );
            }
        }

        private void OnIceConnectionStateChanged( RTCIceConnectionState state )
        {
            PublishStatus( $"WebRTC IceConnectionState: {state}" );
        }

        private async UniTask WaitForIceGatheringAsync( CancellationToken cancellationToken )
        {
            double deadline = Time.realtimeSinceStartupAsDouble + iceGatheringTimeoutSeconds;

            while( _peerConnection != null &&
                   _peerConnection.GatheringState != RTCIceGatheringState.Complete )
            {
                cancellationToken.ThrowIfCancellationRequested();
                if( Time.realtimeSinceStartupAsDouble >= deadline )
                {
                    throw new TimeoutException( "WebRTC ICE Gathering 시간 초과" );
                }
                await UniTask.Yield( PlayerLoopTiming.Update, cancellationToken );
            }
        }

        private async UniTask WaitForConnectionAsync( CancellationToken cancellationToken )
        {
            double deadline = Time.realtimeSinceStartupAsDouble + connectionTimeoutSeconds;

            while( _peerConnection != null )
            {
                cancellationToken .ThrowIfCancellationRequested();
                RTCPeerConnectionState state = _peerConnection.ConnectionState;
                if( state == RTCPeerConnectionState.Connected )
                {
                    return;
                }
                if( state == RTCPeerConnectionState.Failed || state == RTCPeerConnectionState.Closed )
                {
                    throw new InvalidOperationException( $"WebRTC 연결 실패: {state}" );
                }
                if( Time.realtimeSinceStartupAsDouble >= deadline )
                {
                    throw new TimeoutException( "WebRTC Peer 연결 시간 초과" );
                }
                await UniTask.Delay( 100, cancellationToken: cancellationToken );
            }

            throw new InvalidOperationException( "RTCPeerConnection이 종료되었습니다." );
        }

        private async UniTask ResetSessionAsync( string endpoint, string token, CancellationToken cancellationToken )
        {
            var requestData = new VideoSessionRequest { sessionId = sessionId };

            AgentApiResponse response = await PostJsonAsync<VideoSessionRequest, AgentApiResponse>( endpoint, token, "/api/video/session/reset", requestData, cancellationToken);

            if( response == null || response.success == false )
            {
                throw new InvalidOperationException( response?.message ?? "Agent 영상 세션 초기화 실패" );
            }
        }

        private async UniTask PostOfferAsync( string endpoint, string token, string sdp, CancellationToken cancellationToken )
        {
            var requestData = new VideoSessionDescriptionRequest { sessionId = sessionId, type = "offer", sdp = sdp };
            AgentApiResponse response = await PostJsonAsync<VideoSessionDescriptionRequest, AgentApiResponse>( endpoint, token, "/api/video/offer", requestData, cancellationToken);
            if( response == null || !response.success )
            {
                throw new InvalidOperationException( response?.message ?? "Agent Offer 등록 실패" );
            }
        }

        private async UniTask< VideoSessionDescriptionResponse> WaitForAnswerAsync( string endpoint, string token, CancellationToken cancellationToken )
        {
            double deadline = Time.realtimeSinceStartupAsDouble + answerTimeoutSeconds;
            string encodedSessionId = UnityWebRequest.EscapeURL( sessionId );

            while( true )
            {
                cancellationToken .ThrowIfCancellationRequested();
                string path = "/api/video/answer?sessionId={encodedSessionId}";
                VideoSessionDescriptionResponse response = await GetJsonAsync<VideoSessionDescriptionResponse>( endpoint, token, path, cancellationToken);

                if( response != null && response.success && response.hasValue && string.IsNullOrWhiteSpace( response.sdp ) == false )
                {
                    return response;
                }
                if( Time.realtimeSinceStartupAsDouble >= deadline )
                {
                    throw new TimeoutException( "PC Browser Source의 SDP Answer 대기 시간 초과" );
                }
                await UniTask.Delay( answerPollingMilliseconds, cancellationToken: cancellationToken );
            }
        }

        private async UniTask<TResponse> PostJsonAsync<TRequest, TResponse>( string endpoint, string token, string path, TRequest requestData, CancellationToken cancellationToken )
        {
            cancellationToken .ThrowIfCancellationRequested();
            string url = endpoint.TrimEnd('/') + path;
            string json = JsonUtility.ToJson(requestData);
            byte[] body = Encoding.UTF8.GetBytes(json);
            using( var request = new UnityWebRequest( url, UnityWebRequest.kHttpVerbPOST ) )
            {
                request.uploadHandler = new UploadHandlerRaw( body );
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader( "Authorization", $"Bearer {token}" );
                request.SetRequestHeader( "Content-Type", "application/json" );
                request.timeout = Mathf.Max( 1, requestTimeoutSeconds );
                await request.SendWebRequest();
                cancellationToken.ThrowIfCancellationRequested();
                ThrowIfRequestFailed( request );
                return JsonUtility.FromJson<TResponse>( request.downloadHandler.text );
            }
        }

        private async UniTask<TResponse> GetJsonAsync<TResponse>( string endpoint, string token, string path, CancellationToken cancellationToken )
        {
            cancellationToken .ThrowIfCancellationRequested();
            string url = endpoint.TrimEnd('/') + path;
            using( UnityWebRequest request = UnityWebRequest.Get( url ) )
            {
                request.SetRequestHeader( "Authorization", $"Bearer {token}" );
                request.timeout = Mathf.Max( 1, requestTimeoutSeconds );
                await request.SendWebRequest();
                cancellationToken .ThrowIfCancellationRequested();
                ThrowIfRequestFailed( request );
                return JsonUtility .FromJson<TResponse>( request.downloadHandler.text );
            }
        }

        private static void ThrowIfRequestFailed( UnityWebRequest request )
        {
            if( request.result == UnityWebRequest.Result.Success )
            {
                return;
            }
            throw new InvalidOperationException( "Agent HTTP 요청 실패\nURL: {request.url}\nHTTP: {request.responseCode}\nError: {request.error}\nBody: {request.downloadHandler?.text}" );
        }

        private static async UniTask WaitOperationAsync( RTCSessionDescriptionAsyncOperation operation, CancellationToken cancellationToken )
        {
            await UniTask.WaitUntil( () => operation.IsDone, cancellationToken: cancellationToken );
            if( operation.IsError )
            {
                throw new InvalidOperationException( "WebRTC Session Description 작업 실패\nType: {operation.Error.errorType}\nMessage: {operation.Error.message}" );
            }
        }

        private static async UniTask WaitOperationAsync( RTCSetSessionDescriptionAsyncOperation operation, CancellationToken cancellationToken )
        {
            await UniTask.WaitUntil( () => operation.IsDone, cancellationToken: cancellationToken );
            if( operation.IsError )
            {
                throw new InvalidOperationException( "WebRTC SDP 적용 실패\nType: {operation.Error.errorType}\nMessage: {operation.Error.message}" );
            }
        }

        private void DisposePeerConnection()
        {
            videoTrackProvider?.DisposeTrack();
            if( _peerConnection == null )
            {
                return;
            }
            _peerConnection.OnConnectionStateChange = null;

            _peerConnection.OnIceConnectionChange = null;

            _peerConnection.Dispose();
            _peerConnection = null;
        }

        private void ValidateConfiguration( string endpoint, string token )
        {
            if( videoTrackProvider == null )
            {
                throw new InvalidOperationException( "VideoTrackProvider가 연결되지 않았습니다." );
            }

            if( string.IsNullOrWhiteSpace( endpoint ) )
            {
                throw new InvalidOperationException( "Agent Endpoint가 비어 있습니다." );
            }

            if( string.IsNullOrWhiteSpace( token ) || token.Length < 16 )
            {
                throw new InvalidOperationException( "Agent Token이 올바르지 않습니다." );
            }

            if( string.IsNullOrWhiteSpace( sessionId ) )
            {
                throw new InvalidOperationException( "WebRTC Session ID가 비어 있습니다." );
            }

            if( bitrate <= 0 || frameRate <= 0 )
            {
                throw new InvalidOperationException( "Bitrate와 FrameRate는 0보다 커야 합니다." );
            }
        }

        private static string NormalizeEndpoint( string endpoint )
        {
            string value = endpoint.Trim().TrimEnd('/');
            if( !value.Contains( "://" ) )
            {
                value = $"http://{value}";
            }
            if( Uri.TryCreate( value, UriKind.Absolute, out Uri uri ) == false )
            {
                throw new InvalidOperationException( "Agent Endpoint 형식이 올바르지 않습니다." );
            }
            return uri .GetLeftPart( UriPartial.Authority ) .TrimEnd( '/' );
        }

        private void PublishStatus( string message )
        {
            var msg = $"[WebRTC] {message}";
            Debug.Log( msg, this );
            _onMessageChanged.OnNext( msg );
        }

        private void OnDestroy()
        {
            StopVideoLink();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            bitrate = Mathf.Max( 1, bitrate );
            frameRate = Mathf.Max( 1, frameRate );
            requestTimeoutSeconds = Mathf.Max( 1, requestTimeoutSeconds );
            iceGatheringTimeoutSeconds = Mathf.Max( 1, iceGatheringTimeoutSeconds );
            answerTimeoutSeconds = Mathf.Max( 1, answerTimeoutSeconds );
            connectionTimeoutSeconds = Mathf.Max( 1, connectionTimeoutSeconds );
            answerPollingMilliseconds = Mathf.Max( 100, answerPollingMilliseconds );
        }
#endif
    }
}