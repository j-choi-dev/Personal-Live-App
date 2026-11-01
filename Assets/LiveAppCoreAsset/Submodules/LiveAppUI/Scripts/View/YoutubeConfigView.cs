using LiveAppUI.Presenter;
using System;
using TMPro;
using UniRx;
using UnityEngine;

namespace LiveAppUI.View
{
    public sealed class YoutubeConfigView : MonoBehaviour, IYoutubeConfigView
    {
        [SerializeField] private ObservableInput _titleInput;

        [SerializeField] private ObservableDropdown _resolutionDropdown;

        [SerializeField] private ObservableInput _streamKeyInput;

        [SerializeField] private ObservableButton _prepareButton;

        [SerializeField] private ObservableButton _startButton;

        [SerializeField] private ObservableButton _stopButton;

        [SerializeField] private TMP_Text _statusText;

        public IObservable<Unit> OnPrepareButton => _prepareButton.OnClick;
        public IObservable<Unit> OnStartButton => _startButton.OnClick;
        public IObservable<Unit> OnStopButton => _stopButton.OnClick;
        public string Title => _titleInput.Text.Trim();
        public string StreamKey => _streamKeyInput.Text.Trim();

        private void Awake()
        {
            SetIdleStatus();
        }

        public void SetStreamKeyWithoutNotify( string key )
        {
            _streamKeyInput.SetTextWithoutNotify( key );
        }

        public void GetResolution( out int width, out int height )
        {
            if( _resolutionDropdown.Value == 0 )
            {
                width = 1920;
                height = 1080;
            }
            else
            {
                width = 1080;
                height = 1920;
            }
        }

        public void SetIdleStatus()
        {
            _prepareButton.Interactable = true;
            _startButton.Interactable = false;
            _stopButton.Interactable = false;
            _statusText.text = "방송 설정 대기";
        }

        public void SetPreparing( string message )
        {
            _prepareButton.Interactable = false;
            _startButton.Interactable = false;
            _stopButton.Interactable = false;
            _statusText.text = message;
        }

        public void SetReady( string message )
        {
            _prepareButton.Interactable = false;
            _startButton.Interactable = true;
            _stopButton.Interactable = false;
            _statusText.text = message;
        }

        public void SetStarting( string message )
        {
            _prepareButton.Interactable = false;
            _startButton.Interactable = false;
            _stopButton.Interactable = false;
            _statusText.text = message;
        }

        public void SetLive( string message )
        {
            _prepareButton.Interactable = false;
            _startButton.Interactable = false;
            _stopButton.Interactable = true;
            _statusText.text = message;
        }

        public void SetFailed( string message )
        {
            _prepareButton.Interactable = true;
            _startButton.Interactable = false;
            _stopButton.Interactable = true;
            _statusText.text = message;
        }
    }
}