using Cysharp.Threading.Tasks;
using LiveAppUI.Model;
using StudioRendererSDK.Domain;
using UniRx;
using UnityEngine;
using Zenject;

namespace LiveAppUI.Presenter
{
    public sealed class ObsVideoLinkPresenter : MonoBehaviour
    {
        private IObsAgentModel _agentModel;
        private IConfigMenuView _configMenuView;

        [Inject]
        public void Initialize( IConfigMenuView configMenuView,
            IObsAgentModel agentModel )
        {
            _configMenuView = configMenuView;
            _agentModel = agentModel;
        }

        private void Awake()
        {
            SubscribeView();
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
            _agentModel.OnSystemMessageChanged
                .Subscribe( msg => _configMenuView.AddLogText( msg ) )
                .AddTo( this );
            _agentModel.OnConnectionChanged
                .Subscribe( isVal => Debug.Log( $"OnConnectionChanged ... {isVal}" ) )
                .AddTo( this );
        }

        private void SetVideoLink( bool isActive )
        {
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