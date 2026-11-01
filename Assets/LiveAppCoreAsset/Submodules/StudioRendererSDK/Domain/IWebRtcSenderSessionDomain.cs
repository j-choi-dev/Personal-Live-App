using Cysharp.Threading.Tasks;
using System;

namespace StudioRendererSDK.Domain
{
    public interface IWebRtcSenderSessionDomain
    {
        IObservable<string> OnMessageChanged { get; }
        IObservable<bool> OnConnectionChanged { get; }

        UniTask<bool> StartVideoLinkAsync( string endpoint, string token );
        void StopVideoLink();
    }
}
