using Cysharp.Threading.Tasks;
using System;

namespace LiveAppUI.Model
{
    public interface IObsAgentModel
    {
        IObservable<string> OnSystemMessageChanged { get; }
        IObservable<string> OnEndPointChanged { get; }
        IObservable<string> OnAgentTokenChanged { get; }

        IObservable<bool> OnAgentConnectionChanged { get; }
        IObservable<bool> OnRendererConnectionChanged { get; }
        IObservable<bool> OnStreamingChanged { get; }
        IObservable<bool> OnRecordingChanged { get; }

        UniTask<bool> AgentConnectProcess( string endPoint, string token );
        UniTask<bool> StreamingProcess(bool isStart );
        UniTask<bool> RecordingProcess(bool isStart );
        UniTask<bool> StartVideoLinkAsync( string endPoint, string token );
        void StopVideoLink();
    }
}
