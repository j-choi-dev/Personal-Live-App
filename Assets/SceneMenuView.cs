using Cysharp.Threading.Tasks;
using LiveAppUI.Model;
using LiveAppUI.Presenter;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;

namespace LiveAppUI.View
{
    public class SceneMenuView : MonoBehaviour
    {
        [SerializeField] private List<ButtonViewPair> _viewPairs = null;
        [SerializeField] private ObservableButton _loginButton = null;

        private IServerModalView _loginView;
        private IRoomModalView _roomView;

        [Inject]
        public void Initialize( IServerModalView loginView,
            IRoomModalView roomView )
        {
            _loginView = loginView;
            _roomView = roomView;
        }

        private void Awake()
        {
            for( int i = 0; i < _viewPairs.Count; i++ )
            {
                var viewPair = _viewPairs[i];
                viewPair.view.SetActive( false );
                viewPair.button.OnClick
                    .Subscribe( isOn =>
                    {
                        CloaseAllTab();
                        viewPair.view.SetActive( true );
                    } )
                    .AddTo( this );
            }
            CloaseAllTab();

            _loginButton.OnClick
                .Subscribe(x =>
                {
                    _roomView.SetActive( true );
                    _loginView.SetActive( false );
                } )
                .AddTo( this );
        }

        private void CloaseAllTab()
        {
            for( int i = 0; i < _viewPairs.Count; i++ )
            {
                _viewPairs[i].view.SetActive( false );
            }
        }
    }
}