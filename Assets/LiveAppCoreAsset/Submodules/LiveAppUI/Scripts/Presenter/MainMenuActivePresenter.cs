using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;
using UniRx;

namespace LiveAppUI.Presenter
{
    public class MainMenuActivePresenter : MonoBehaviour
    {
        private IMainMenuView _mainMenuView;
        private IResourceMenuView _resourceMenuView;
        private IConfigMenuView _configMenuView;

        [Inject]
        public void Initialize( IMainMenuView mainMenuView,
            IResourceMenuView resourceMenuView,
            IConfigMenuView configMenuView)
        {
            _mainMenuView = mainMenuView;
            _resourceMenuView = resourceMenuView;
            _configMenuView = configMenuView;
        }

        private void Awake()
        {
            _mainMenuView.OnResourceButtonClick
                .Subscribe( x =>
                {
                    CloaseAllTab();
                    _resourceMenuView.SetActive( true );
                } )
                .AddTo( this );

            _mainMenuView.OnConfigButtonClick
                .Subscribe( x =>
                {
                    CloaseAllTab();
                    _mainMenuView.SetActive( true );
                    _configMenuView.SetActive( true );
                } )
                .AddTo( this );

            _resourceMenuView.OnBackButtonClick
                .Subscribe( x =>
                {
                    if( _mainMenuView.IsActive )
                    {
                        return;
                    }
                    CloaseAllTab();
                    _mainMenuView.SetActive( true );
                } )
                .AddTo( this );
        }

        private void Start()
        {
            CloaseAllTab();
            _mainMenuView.SetActive( true );
        }

        private void CloaseAllTab()
        {
            _mainMenuView.SetActive( false );
            _resourceMenuView.SetActive( false );
            _configMenuView.SetActive( false );
        }
    }
}
