using Cysharp.Threading.Tasks;
using LiveAppUI.Presenter;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;

namespace LiveAppUI.View
{
    public class LiveAppMainMenuView : MonoBehaviour
    {
        [SerializeField] private List<ButtonViewPair> _viewPairs = null;
        [SerializeField] private ObservableButton _loginButton = null;

        [SerializeField] private ObservableButton _resourceButton = null;
        [SerializeField] private GameObject _resourceView = null;

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
            _resourceButton.OnClick
                .Subscribe( _ =>
                {
                    CloaseAllTab();
                    _resourceView.SetActive( !_resourceView.activeSelf );
                } )
                .AddTo( this );
            //for( int i = 0; i < _viewPairs.Count; i++ )
            //{
            //    var viewPair = _viewPairs[i];
            //    viewPair.view.SetActive( false );
            //    viewPair.button.OnClick
            //        .Subscribe( isOn =>
            //        {
            //            CloaseAllTab();
            //            if( viewPair.view.activeSelf )
            //            {
            //                viewPair.view.SetActive( false );
            //            }
            //            else
            //            {
            //                viewPair.view.SetActive( true );
            //            }
            //        } )
            //        .AddTo( this );
            //}
            CloaseAllTab();

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
            _resourceView.SetActive( false );
        }

        private void CloaseAllTab()
        {
            _resourceView.SetActive( false );
        }
    }
}