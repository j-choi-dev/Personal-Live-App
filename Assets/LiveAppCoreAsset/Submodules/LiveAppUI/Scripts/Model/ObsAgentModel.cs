using Cysharp.Threading.Tasks;
using StudioRendererSDK.Application;
using System;
using UniRx;

namespace LiveAppUI.Model
{
    public class ObsAgentModel : IObsAgentModel
    {
        private IObsAgentContext _obsAgentContext;
        private IRenderSendContext _renderSendContext;

        public IObservable<string> OnSystemMessageChanged => _obsAgentContext.OnSystemMessageChanged.Merge( _renderSendContext.OnMessageChanged );
        public IObservable<string> OnEndPointChanged => _obsAgentContext.OnEndPointChanged;
        public IObservable<string> OnAgentTokenChanged => _obsAgentContext.OnAgentTokenChanged;

        public IObservable<bool> OnAgentConnectionChanged => _obsAgentContext.OnAgentConnectionChanged;
        public IObservable<bool> OnRendererConnectionChanged => _renderSendContext.OnRendererConnectionChanged;
        public IObservable<bool> OnStreamingChanged => _obsAgentContext.OnStreamingChanged;
        public IObservable<bool> OnRecordingChanged => _obsAgentContext.OnRecordingChanged;

        public ObsAgentModel( IObsAgentContext obsAgentContext, 
            IRenderSendContext renderSendContext )
        {
            _obsAgentContext = obsAgentContext;
            _renderSendContext = renderSendContext;
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

        public async UniTask<bool> StartVideoLinkAsync( string endPoint, string token )
            => await _renderSendContext.StartVideoLinkAsync( endPoint, token );

        public void StopVideoLink()
            => _renderSendContext.StopVideoLink();
    }
}
