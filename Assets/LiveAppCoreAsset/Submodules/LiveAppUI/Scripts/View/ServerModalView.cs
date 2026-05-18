using LiveAppUI.Presenter;
using System;
using System.Collections.Generic;
using UnityEngine;
using UniRx;


namespace LiveAppUI.View
{
    public class ServerModalView : MonoBehaviour, IServerModalView
    {
        [SerializeField]
        private ObservableDropdownTMPro _dropdown;
        [SerializeField]
        private ObservableInputTMPro _id;
        [SerializeField]
        private ObservableInputTMPro _password;
        [SerializeField]
        private ObservableButton _closeButton;
        [SerializeField]
        private ObservableButton _loginButton;


        public bool IsActive => gameObject.activeSelf;

        public IObservable<Unit> OnClose => _closeButton.OnClick;

        public IObservable<Unit> OnClicLogin => _loginButton.OnClick;
        public string CurrentID => _id.Text;

        public string CurrentPassword => _password.Text;

        public int CurrentIndex => _dropdown.Value;

        private void Awake()
        {
            _id.OnValueChanged
                .Merge( _password.OnValueChanged )
                .Subscribe( x => _loginButton.Interactable = !( string.IsNullOrEmpty(_id.Text) || string.IsNullOrEmpty(_password.Text) ) )
                .AddTo( this );
        }

        private void Start()
        {
            SetActive( true );
            _loginButton.Interactable = false;
        }

        public void SetServerList( IReadOnlyList<string> list )
        {
            _dropdown.SetOptions( list );
        }

        public void SetActive( bool isActive )
        {
            gameObject.SetActive( isActive );
        }
    }
}
