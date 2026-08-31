using Cysharp.Threading.Tasks;
using LiveAppUI.Presenter;
using System;
using UniRx;
using UnityEngine;
using Zenject;

namespace LiveAppUI.View
{
    public class MainMenuView : MonoBehaviour, IMainMenuView
    {
        [SerializeField] private ObservableButton _loginButton = null;
        [SerializeField] private ObservableButton _resourceButton = null;
        [SerializeField] private ObservableButton _configButton = null;
        [SerializeField] private ResourceMenuView _resourceView = null;
        [SerializeField] private ConfigMenuView _configView = null;

        private IServerModalView _loginView;
        private IRoomModalView _roomView;

        public IObservable<Unit> OnResourceButtonClick => _resourceButton.OnClick;
        public IObservable<Unit> OnConfigButtonClick => _configButton.OnClick;

        public bool IsActive => gameObject.activeSelf;

        [Inject]
        public void Initialize( IServerModalView loginView,
            IRoomModalView roomView )
        {
            _loginView = loginView;
            _roomView = roomView;
        }

        private void Awake()
        {
            _loginButton.OnClick
                .Subscribe(x =>
                {
                    _roomView.SetActive( true );
                    _loginView.SetActive( false );
                } )
                .AddTo( this );
        }

        private void Start()
        {
            _resourceView.gameObject.SetActive( false );
            _configView.gameObject.SetActive( false );
        }

        public void SetActive( bool isActive )
        {
            gameObject.SetActive( isActive );
        }
    }
}