using StudioCharacterSDK.Domain;
using StudioResourceSDK.Application;
using StudioTrackingSDK.Application;
using System;
using UniRx;

namespace LiveApp
{
    public class ARKitBodyTrackingModel : IARKitBodyTrackingModel, IDisposable
    {
        private IBodyTrackingContext _trackingContext;
        private ISceneResourceListContext _sceneResourceListContext;
        private IBodyData _currentCharacter = null;
        private CompositeDisposable _disposables = new CompositeDisposable();

        public ARKitBodyTrackingModel(IBodyTrackingContext trackingContext,
            ISceneResourceListContext sceneResourceListContext)
        {
            _trackingContext = trackingContext;
            _sceneResourceListContext = sceneResourceListContext;

            SubscribeTrackingData();

            // TODO 리팩터링 대상!!! @Choi 26.10.26
            _sceneResourceListContext.OnCurrentCharacterChanged
                .Subscribe(arg =>
                {
                    var target = arg as IBodyData;
                    if (target == null)
                    {
                        UnityEngine.Debug.LogError($"Invalid Resource ; Not Exist Body Component({arg.ID})");
                        _trackingContext.SetCharacterID(string.Empty);
                        _trackingContext.SetIsActive(false);
                        _currentCharacter = null;
                        return;
                    }
                    _trackingContext.SetCharacterID(arg.ID);
                    _trackingContext.SetIsActive(true);
                    _currentCharacter = target;
                })
                .AddTo(_disposables);
        }

        public bool IsAbleTracking => _trackingContext.IsActive;

        public float Intensity { get; private set; }

        public void Dispose()
        {
            _disposables.Dispose();
            _disposables = null;
        }

        public void SetAbleTracking(bool isOK)
            => _trackingContext.SetIsActive(isOK);

        public void SetIntensity(float intensity)
            => Intensity = intensity;

        private void SubscribeTrackingData()
        {
            _trackingContext.OnBodyAngleX
                .Subscribe(val => _currentCharacter.SetBodyAngleX(val) )
                .AddTo(_disposables);
            _trackingContext.OnBodyAngleY
                .Subscribe(val => _currentCharacter.SetBodyAngleY(val) )
                .AddTo(_disposables);
            _trackingContext.OnBodyAngleZ
                .Subscribe(val => _currentCharacter.SetBodyAngleZ(val) )
                .AddTo(_disposables);
        }
    }
}
