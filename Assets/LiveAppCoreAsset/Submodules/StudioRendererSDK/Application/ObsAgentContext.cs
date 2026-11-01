using Cysharp.Threading.Tasks;
using StudioRendererSDK.Domain;
using System;

namespace StudioRendererSDK.Application
{
    public class ObsAgentContext : IObsAgentContext
    {
        private IObsAgentControlDomain _obsAgentControlDomain;
        public IObservable<string> OnSystemMessageChanged => _obsAgentControlDomain.OnSystemMessageChanged;
        public IObservable<string> OnEndPointChanged => _obsAgentControlDomain.OnEndPointChanged;
        public IObservable<string> OnAgentTokenChanged => _obsAgentControlDomain.OnAgentTokenChanged;
        public IObservable<string> OnStreamKeyChanged => _obsAgentControlDomain.OnStreamKeyChanged;

        public IObservable<bool> OnAgentConnectionChanged => _obsAgentControlDomain.OnConnectionChanged;
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

        public async UniTask<bool> PrepareYoutubeLiveProcess( YoutubeLivePrepareRequest request )
        {
            return await _obsAgentControlDomain.PrepareYoutubeLiveProcess( request );
        }
        public async UniTask<bool> StartYoutubeLiveProcess()
        {
            return await _obsAgentControlDomain.StartYoutubeLiveProcess();
        }
        public async UniTask<bool> StopYoutubeLiveProcess()
        {
            return await _obsAgentControlDomain.StopYoutubeLiveProcess();
        }
        public async UniTask<YoutubeLiveStatusResponse> GetYoutubeLiveStatusProcess()
        {
            return await _obsAgentControlDomain.GetYoutubeLiveStatusProcess();
        }
    }
}
