using Cysharp.Threading.Tasks;
using System;

namespace StudioRendererSDK.Application
{
    public interface IObsAgentContext
    {
        IObservable<string> OnSystemMessageChanged { get; }
        IObservable<string> OnEndPointChanged { get; }
        IObservable<string> OnAgentTokenChanged { get; }

        IObservable<bool> OnConnectionChanged { get; }
        IObservable<bool> OnStreamingChanged { get; }
        IObservable<bool> OnRecordingChanged { get; }

        UniTask<bool> AgentConnectProcess( string endPoint, string token );
        UniTask<bool> StartStreamingProcess();
        UniTask<bool> StopStreamingProcess();
        UniTask<bool> StartRecordingProcess();
        UniTask<bool> StopRecordingProcess();
    }
}
