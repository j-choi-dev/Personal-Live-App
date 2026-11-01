using StudioRendererSDK.Domain;
using System;
using Unity.WebRTC;
using UnityEngine;

namespace StudioRendererSDK.Infrastructure
{
    // @Use
    [DisallowMultipleComponent]
    public sealed class ObsWebRtcVideoTrackProvider : MonoBehaviour
    {
        [Header("Required Reference")]
        [SerializeField] private MonoBehaviour videoSourceComponent;

        private IObsVideoSource _videoSource;

        private RTCPeerConnection _peerConnection;
        private VideoStreamTrack _videoTrack;
        private RTCRtpSender _sender;

        public VideoStreamTrack VideoTrack => _videoTrack;

        public RTCRtpSender Sender => _sender;

        public bool IsCreated => _videoTrack != null && _sender != null;

        private void Awake()
        {
            _videoSource = videoSourceComponent as IObsVideoSource;

            if( _videoSource == null )
            {
                Debug.LogError( $"{nameof( ObsWebRtcVideoTrackProvider )}: {nameof( videoSourceComponent )}가 {nameof( IObsVideoSource )}를 구현하지 않았습니다.", this );

                enabled = false;
            }
        }

        public VideoStreamTrack CreateTrack( RTCPeerConnection peerConnection, int bitrate, int frameRate )
        {
            if( !enabled )
            {
                throw new InvalidOperationException( "WebRTC VideoTrackProvider가 비활성화되어 있습니다." );
            }
            if( peerConnection == null )
            {
                throw new ArgumentNullException( nameof( peerConnection ) );
            }
            if( _videoSource == null || !_videoSource.IsReady || _videoSource.OutputTexture == null )
            {
                throw new InvalidOperationException( "OBS RenderTexture가 준비되지 않았습니다." );
            }
            if( bitrate <= 0 )
            {
                throw new ArgumentOutOfRangeException( nameof( bitrate ), "Bitrate는 0보다 커야 합니다." );
            }
            if( frameRate <= 0 )
            {
                throw new ArgumentOutOfRangeException( nameof( frameRate ), "FrameRate는 0보다 커야 합니다." );
            }
            DisposeTrack();
            _peerConnection = peerConnection;
            Texture sourceTexture = _videoSource.OutputTexture;
            var actualFormat = sourceTexture.graphicsFormat;

            var supportedFormat = WebRTC.GetSupportedGraphicsFormat( SystemInfo.graphicsDeviceType );
            if( actualFormat != supportedFormat )
            {
                throw new InvalidOperationException( "WebRTC 영상 소스 Graphics Format이 현재 실행 환경에서 지원되지 않습니다.\n" +
                    $"Graphics Device: {SystemInfo.graphicsDeviceType}\n" +
                    $"Current Format: {actualFormat}\n" +
                    $"Required Format: {supportedFormat}" );
            }
            _videoTrack = new VideoStreamTrack( sourceTexture );
            _sender = _peerConnection.AddTrack( _videoTrack );
            Debug.Log( $"[WebRTC Track]\nVideoTrack Created: {_videoTrack != null}\n" +
                $"Sender Created: {_sender != null}\n" +
                $"Texture: {sourceTexture.name}\n" +
                $"Format: {sourceTexture.graphicsFormat}\n" +
                $"Size: {sourceTexture.width}x{sourceTexture.height}",
                this );
            return _videoTrack;
        }

        private void ApplyEncodingParameters( int bitrate, int frameRate )
        {
            RTCRtpSendParameters parameters = _sender.GetParameters();

            if( parameters.encodings == null ||
                parameters.encodings.Length == 0 )
            {
                Debug.LogWarning( "WebRTC Sender에 Encoding Parameter가 없습니다.", this );
                return;
            }

            ulong convertedBitrate = checked((ulong)bitrate);
            uint convertedFrameRate = checked((uint)frameRate);

            foreach( RTCRtpEncodingParameters encoding in parameters.encodings )
            {
                if( encoding == null )
                {
                    continue;
                }
                encoding.maxBitrate = convertedBitrate;
                encoding.maxFramerate = convertedFrameRate;
            }
            RTCError error = _sender.SetParameters(parameters);
            if( error.errorType != RTCErrorType.None )
            {
                throw new InvalidOperationException( "WebRTC 송신 설정 실패\nType: {error.errorType}\nMessage: {error.message}" );
            }
        }

        public void DisposeTrack()
        {
            if( _peerConnection != null && _sender != null )
            {
                try
                {
                    _peerConnection.RemoveTrack( _sender );
                }
                catch( Exception exception )
                {
                    Debug.LogWarning( $"WebRTC Track 제거 중 경고: {exception.Message}", this );
                }
            }
            _videoTrack?.Dispose();
            _videoTrack = null;
            _sender = null;
            _peerConnection = null;
        }

        private void OnDestroy()
        {
            DisposeTrack();
        }
    }
}