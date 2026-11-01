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

        private bool _bodyTrackingEnabled;

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
                        _trackingContext.SetCharacterID(string.Empty);
                        _trackingContext.SetIsActive(false);
                        _currentCharacter = null;
                        return;
                    }
                    _currentCharacter = target;
                    _trackingContext.SetCharacterID(arg.ID);

                    ApplyTrackingState();
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

        public void ResetTracking()
        {
            _trackingContext.ResetTracking();
        }

        public void SetAbleTracking(bool isOK)
        {
            _bodyTrackingEnabled = isOK;
            ApplyTrackingState();
        }

        private void ApplyTrackingState()
        {
            var active = _bodyTrackingEnabled && _currentCharacter != null;

            _trackingContext.SetIsActive(active);

            if (!active)
            {
                _currentCharacter?.SetBodyAngleX(0f);
                _currentCharacter?.SetBodyAngleY(0f);
                _currentCharacter?.SetBodyAngleZ(0f);
            }
        }

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
