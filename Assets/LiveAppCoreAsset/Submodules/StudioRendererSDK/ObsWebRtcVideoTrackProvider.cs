using StudioRendererSDK.Domain;
using System;
using Zenject;

namespace StudioRendererSDK.Infrastructure
{
    public sealed class ObsWebRtcVideoTrackProvider
    {
        private readonly IObsVideoSource _videoSource;

        private VideoStreamTrack _videoTrack;
        private RTCRtpSender _sender;

        [Inject]
        public ObsWebRtcVideoTrackProvider(
            IObsVideoSource videoSource )
        {
            _videoSource = videoSource;
        }

        public VideoStreamTrack CreateTrack(
            RTCPeerConnection peerConnection,
            int bitrate,
            int frameRate )
        {
            if( !_videoSource.IsReady )
            {
                throw new InvalidOperationException(
                    "OBS RenderTexture가 준비되지 않았습니다." );
            }

            _videoTrack =
                new VideoStreamTrack(
                    _videoSource.OutputTexture );

            _sender =
                peerConnection.AddTrack( _videoTrack );

            RTCRtpSendParameters parameters =
            _sender.GetParameters();

            foreach( RTCRtpEncodingParameters encoding
                     in parameters.Encodings )
            {
                encoding.maxBitrate = bitrate;
                encoding.maxFramerate = frameRate;
            }

            RTCError error =
            _sender.SetParameters(parameters);

            if( error.errorType != RTCErrorType.None )
            {
                throw new InvalidOperationException(
                    $"WebRTC 송신 설정 실패: {error.errorType}" );
            }

            return _videoTrack;
        }

        public void Dispose()
        {
            _videoTrack?.Dispose();
            _videoTrack = null;
            _sender = null;
        }
    }
}