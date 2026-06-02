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

        [Inject]
        public void Initialize( IMainMenuView mainMenuView,
            IResourceMenuView resourceMenuView )
        {
            _mainMenuView = mainMenuView;
            _resourceMenuView = resourceMenuView;
        }

        private void Awake()
        {
            _mainMenuView.OnResourceButtonCLick
                .Subscribe( x =>
                {
                    CloaseAllTab();
                    _resourceMenuView.SetActive( true );
                } )
                .AddTo( this );

            _resourceMenuView.OnBackButtonClick
                .Subscribe( x =>
                {
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
        }
    }
}
