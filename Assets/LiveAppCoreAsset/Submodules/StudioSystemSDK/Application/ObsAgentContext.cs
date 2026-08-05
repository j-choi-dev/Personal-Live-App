using Cysharp.Threading.Tasks;
using StudioSystemSDK.Domain;
using System;

namespace StudioSystemSDK.Context
{
    public class ObsAgentContext : IObsAgentContext
    {
        private IObsAgentControlDomain _obsAgentControlDomain;
        public IObservable<string> OnSystemMessageChanged => _obsAgentControlDomain.OnSystemMessageChanged;
        public IObservable<string> OnEndPointChanged => _obsAgentControlDomain.OnEndPointChanged;
        public IObservable<string> OnAgentTokenChanged => _obsAgentControlDomain.OnAgentTokenChanged;

        public IObservable<bool> OnConnectionChanged => _obsAgentControlDomain.OnConnectionChanged;
        public IObservable<bool> OnStreamingChanged => _obsAgentControlDomain.OnStreamingChanged;
        public IObservable<bool> OnRecordingChanged => _obsAgentControlDomain.OnRecordingChanged;

        public ObsAgentContext(IObsAgentControlDomain obsAgentControlDomain )
        {
            _obsAgentControlDomain = obsAgentControlDomain;
        }

        public async UniTask<bool> AgentConnectProcess( string endPoint, string token )
        {
            return await _obsAgentControlDomain.AgentConnectProcess( endPoint, token );
        }

        public async UniTask<bool> StartRecordingProcess()
        {
            return await _obsAgentControlDomain.StartRecordingProcess();
        }

        public async UniTask<bool> StopRecordingProcess()
        {
            return await _obsAgentControlDomain.StopRecordingProcess();
        }

        public async UniTask<bool> StartStreamingProcess()
        {
            return await _obsAgentControlDomain.StartStreamingProcess();
        }

        public async UniTask<bool> StopStreamingProcess()
        {
            return await _obsAgentControlDomain.StopStreamingProcess();
        }
    }
}
