using UnityEngine;
using Zenject;
using UniRx;

namespace LiveAppUI.Presenter
{
    public class SystemUIPresenter : MonoBehaviour
    {
        private IMainMenuView _mainMenuView;

        [Inject]
        public void Initialize(
            IMainMenuView mainMenuView )
        {
            _mainMenuView = mainMenuView;
        }

        private void Awake()
        {
            _mainMenuView.OnRecordingChanged
                .Subscribe( arg => Debug.Log( $"OnRecordingChanged ... {arg}" ) )
                .AddTo( this );
            _mainMenuView.OnClickEmergency
                .Subscribe( arg => Debug.Log( $"OnClickEmergency ... {arg}" ) )
                .AddTo( this );
        }
    }
}