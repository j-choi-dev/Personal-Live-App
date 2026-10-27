using Cysharp.Threading.Tasks;
using LiveAppCore.Domain;
using UnityEngine;
using Zenject;
using UniRx;
using LiveApp;

namespace LiveAppUI.Presenter
{
    public class SystemMenuPresenter : MonoBehaviour
    {
        private ISystemMenuView _systemMenuView;
        private IARKitFacialTrackingModel _facialTrackingModel;
        private IARKitBodyTrackingModel _bodyTrackingModel;

        [Inject]
        public void Initialize( ISystemMenuView systemMenuView,
            IARKitFacialTrackingModel facialTrackingModel,
            IARKitBodyTrackingModel bodyTrackingModel)
        {
            _systemMenuView = systemMenuView;
            _facialTrackingModel = facialTrackingModel;
            _bodyTrackingModel = bodyTrackingModel;
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
            _systemMenuView.OnClickCalibration
                .Subscribe(_ =>
                {
                    _facialTrackingModel.ResetTracking();
                    _bodyTrackingModel.ResetTracking();
                })
                .AddTo(this);
        }
    }
}
