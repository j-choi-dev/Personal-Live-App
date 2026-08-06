using Cysharp.Threading.Tasks;
using StudioRendererSDK.Application;
using StudioRendererSDK.Domain;
using System;

namespace LiveAppUI.Model
{
    public class ObsAgentModel : IObsAgentModel
    {
        private IObsAgentContext _obsAgentContext;

        public IObservable<string> OnSystemMessageChanged => _obsAgentContext.OnSystemMessageChanged;
        public IObservable<string> OnEndPointChanged => _obsAgentContext.OnEndPointChanged;
        public IObservable<string> OnAgentTokenChanged => _obsAgentContext.OnAgentTokenChanged;

        public IObservable<bool> OnConnectionChanged => _obsAgentContext.OnConnectionChanged;
        public IObservable<bool> OnStreamingChanged => _obsAgentContext.OnStreamingChanged;
        public IObservable<bool> OnRecordingChanged => _obsAgentContext.OnRecordingChanged;

        public ObsAgentModel( IObsAgentContext obsAgentContext)
        {
            _obsAgentContext = obsAgentContext;
        }

        public async UniTask<bool> AgentConnectProcess( string endPoint, string token )
        {
            return await _obsAgentContext.AgentConnectProcess( endPoint, token );
        }

        public async UniTask<bool> RecordingProcess( bool isStart )
        {
            return isStart 
                ? await _obsAgentContext.StartRecordingProcess()
                : await _obsAgentContext.StopRecordingProcess();
        }

        public async UniTask<bool> StreamingProcess( bool isStart )
        {
            return isStart
                ? await _obsAgentContext.StartStreamingProcess()
                : await _obsAgentContext.StopStreamingProcess();
        }
    }
}
