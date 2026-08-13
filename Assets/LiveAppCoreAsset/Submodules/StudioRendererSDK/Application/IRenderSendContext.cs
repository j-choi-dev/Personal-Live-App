using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace StudioRendererSDK.Application
{
    public interface IRenderSendContext
    {
        IObservable<string> OnMessageChanged { get; }
        IObservable<bool> OnRendererConnectionChanged { get; }

        UniTask<bool> StartVideoLinkAsync( string endpoint, string token );
        void StopVideoLink();
    }
}
