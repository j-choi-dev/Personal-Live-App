using Cysharp.Threading.Tasks;
using StudioRendererSDK.Domain;
using System;

namespace StudioRendererSDK.Application
{
    public class RenderSendContext : IRenderSendContext
    {
        private IWebRtcSenderSessionDomain _senderSessionDomain;

        public IObservable<string> OnMessageChanged => _senderSessionDomain.OnMessageChanged;
        public IObservable<bool> OnRendererConnectionChanged => _senderSessionDomain.OnConnectionChanged;

        public RenderSendContext(IWebRtcSenderSessionDomain senderSessionDomain )
        {
            _senderSessionDomain=senderSessionDomain;
        }

        public async UniTask<bool> StartVideoLinkAsync( string endpoint, string token )
            => await _senderSessionDomain.StartVideoLinkAsync( endpoint, token );

        public void StopVideoLink()
            => _senderSessionDomain.StopVideoLink();
    }
}
