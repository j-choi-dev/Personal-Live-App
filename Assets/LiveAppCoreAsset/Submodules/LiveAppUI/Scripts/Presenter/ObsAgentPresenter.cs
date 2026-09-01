using Cysharp.Threading.Tasks;
using LiveAppUI.Model;
using StudioNetworkSDK.Application;
using UnityEngine;
using Zenject;
using UniRx;

namespace LiveAppUI.Presenter
{
    public class ObsAgentPresenter : MonoBehaviour
    {
        private IOBSConfigView _configMenuView;
        private IObsAgentModel _obsAgentModel;

        [Inject]
        public void Initialize( IOBSConfigView configMenuView,
            IObsAgentModel obsAgentModel )
        {
            _configMenuView = configMenuView;
            _obsAgentModel = obsAgentModel;
        }

        private void Awake()
        {
            SubscribeView();
            SubscribeModel();
        }

        private void SubscribeView()
        {
            _configMenuView.OnConnectionChanged
                .Where( isOn => isOn )
                .Subscribe( _ => _obsAgentModel.AgentConnectProcess( _configMenuView.EndPoint, _configMenuView.AgentToken ).Forget() )
                .AddTo( this );
            _configMenuView.OnStreamingChanged
                .Subscribe( isOn => _obsAgentModel.StreamingProcess( isOn ).Forget() )
                .AddTo( this );
            _configMenuView.OnRecordingChanged
                .Subscribe( isOn => _obsAgentModel.RecordingProcess( isOn ).Forget() )
                .AddTo( this );
        }

        private void SubscribeModel()
        {
            _obsAgentModel.OnSystemMessageChanged
                .Subscribe( msg => _configMenuView.AddLogText( msg ) )
                .AddTo( this );
            _obsAgentModel.OnEndPointChanged
                .Subscribe( endpoint =>
                {
                    Debug.Log( $"_configMenuView.SetEndPointWithoutNotify( {endpoint} )" );
                    _configMenuView.SetEndPointWithoutNotify( endpoint );
                } )
                .AddTo( this );

            _obsAgentModel.OnAgentTokenChanged
                .Subscribe( token =>
                {
                    Debug.Log( $"_configMenuView.SetAgentTokenWithoutNotify( {token} )" );
                    _configMenuView.SetAgentTokenWithoutNotify( token );
                } )
                .AddTo( this );
        }
    }
}
