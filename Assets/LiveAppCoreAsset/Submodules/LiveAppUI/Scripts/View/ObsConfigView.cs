using LiveAppUI.Presenter;
using System;
using TMPro;
using UniRx;
using UnityEngine;

namespace LiveAppUI.View
{
    public class ObsConfigView : MonoBehaviour, IOBSConfigView
    {
        [SerializeField] private ObservableInput _endPointInput = null;
        [SerializeField] private ObservableInput _agentTokenInput = null;

        [SerializeField] private ObservableToggle _connectToggle = null;
        [SerializeField] private ObservableToggle _streamingToggle = null;
        [SerializeField] private ObservableToggle _recToggle = null;
        [SerializeField] private TMP_Text _statusText = null;

        public IObservable<bool> OnConnectionChanged => _connectToggle.OnActiveChanged;
        public IObservable<bool> OnStreamingChanged => _streamingToggle.OnActiveChanged;
        public IObservable<bool> OnRecordingChanged => _recToggle.OnActiveChanged;

        public bool IsActive => gameObject.activeSelf;

        public string EndPoint => _endPointInput.Text;
        public string AgentToken => _agentTokenInput.Text;

        public void SetLogText(string text)
            => _statusText.text = text;

        public void AddLogText( string text )
            => _statusText.text = string.Join('\n', _statusText.text, text);

        private void Awake()
        {
            _connectToggle.OnActiveChanged
                .Subscribe( isOn => SetConnectionState( isOn ) )
                .AddTo( this );
            _streamingToggle.OnActiveChanged
                .Subscribe( isOn => SetStreamingState( isOn ) )
                .AddTo( this );
            _recToggle.OnActiveChanged
                .Subscribe( isOn => SetRecordingState( isOn ) )
                .AddTo( this );
        }

        private void Start()
        {
            _connectToggle.SetIsActiveWithoutNotify( false );
            _streamingToggle.SetIsActiveWithoutNotify( false );
            _recToggle.SetIsActiveWithoutNotify( false );

            _connectToggle.Interactable = true;
            _streamingToggle.Interactable = false;
            _recToggle.Interactable = false;

            _statusText.text = string.Empty;
        }

        private void SetConnectionState(bool isActive)
        {
            _streamingToggle.Interactable = isActive;
            _recToggle.Interactable = isActive;
            if(isActive == false)
            {
                _streamingToggle.SetIsActiveWithoutNotify( isActive );
                _recToggle.SetIsActiveWithoutNotify( isActive );
            }
        }

        private void SetStreamingState( bool isActive )
        {
            _recToggle.Interactable = !isActive;
            if(isActive)
            {
                _recToggle.SetIsActiveWithoutNotify( false );
                //_streamingToggle.SetIsActiveWithoutNotify( isActive );
            }
        }

        private void SetRecordingState( bool isActive )
        {
            _streamingToggle.Interactable = !isActive;
            if( isActive )
            {
                _streamingToggle.SetIsActiveWithoutNotify( false );
                //_recToggle.SetIsActiveWithoutNotify( isActive );
            }
        }

        public void SetActive( bool isActive )
            => gameObject.SetActive( isActive );

        public void SetEndPointWithoutNotify( string val )
            => _endPointInput.SetTextWithoutNotify( val );

        public void SetAgentTokenWithoutNotify( string val )
            => _agentTokenInput.SetTextWithoutNotify( val );
    }
}
