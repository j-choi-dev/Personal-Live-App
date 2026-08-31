using Cysharp.Threading.Tasks;
using LiveAppUI.Model;
using UniRx;
using UnityEngine;
using Zenject;

namespace LiveAppUI.Presenter
{
    public sealed class ObsVideoLinkPresenter : MonoBehaviour
    {
        private IObsAgentModel _agentModel;
        private IOBSConfigView _configMenuView;

        [Inject]
        public void Initialize( IOBSConfigView configMenuView,
            IObsAgentModel agentModel )
        {
            _configMenuView = configMenuView;
            _agentModel = agentModel;
        }

        private void Awake()
        {
            //SubscribeView();
            SubscribeModel();
        }

        private void SubscribeView()
        {
            _configMenuView.OnRecordingChanged
                .Subscribe( isOn => SetVideoLink(isOn) )
                .AddTo( this );
            _configMenuView.OnStreamingChanged
                .Subscribe( isOn => SetVideoLink( isOn ) )
                .AddTo( this );
        }

        private void SubscribeModel()
        {
            //_agentModel.OnSystemMessageChanged
            //    .Subscribe( msg => _configMenuView.AddLogText( msg ) )
            //    .AddTo( this );
            _agentModel.OnRendererConnectionChanged
                .Subscribe( isConnected => Debug.Log( $"[WebRTC UI] Renderer Connected: {isConnected}" ) )
                .AddTo( this );
        }

        public void SetVideoLink( bool isActive )
        {
            Debug.Log( $"[WebRTC UI] SetVideoLink({isActive})", this );
            if( isActive )
            {
                _agentModel.StartVideoLinkAsync( _configMenuView.EndPoint, _configMenuView.AgentToken ).Forget();
            }
            else
            {
                _agentModel.StopVideoLink();
            }
        }
    }
}