using Cysharp.Threading.Tasks;
using LiveAppCore.Domain;
using UnityEngine;
using Zenject;
using UniRx;

namespace LiveAppUI.Presenter
{
    public class SystemMenuPresenter : MonoBehaviour
    {
        private ISystemMenuView _systemMenuView;

        [Inject]
        public void Initialize( ISystemMenuView systemMenuView )
        {
            _systemMenuView = systemMenuView;
        }

        private void Awake()
        {
            _systemMenuView.OnClickEmergency
                .Subscribe( _ => Debug.Log( "Emergency" ) )
                .AddTo( this );
            _systemMenuView.OnClickLock
                .Subscribe( _ => Debug.Log( $"Lock : {_}" ) )
                .AddTo( this );
            _systemMenuView.OnClickRec
                .Subscribe( _ => Debug.Log( $"Rec : {_}" ) )
                .AddTo( this );
        }
    }
}
