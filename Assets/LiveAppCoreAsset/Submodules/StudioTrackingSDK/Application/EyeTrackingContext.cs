using StudioTrackingSDK.Domain;
using System;
namespace StudioTrackingSDK.Application
{
    public class EyeTrackingContext : IEyeTrackingContext
    {
        private IEyeTrackingDomain _eyeTrackingDomain;

        public IObservable<float> OnEyeBallAngleX => _eyeTrackingDomain.OnEyeBallAngleX;

        public IObservable<float> OnEyeBallAngleY => _eyeTrackingDomain.OnEyeBallAngleY;

        public IObservable<float> OnEyeBlinkLeft => _eyeTrackingDomain.OnEyeBlinkLeft;

        public IObservable<float> OnEyeBlinkRight => _eyeTrackingDomain.OnEyeBlinkRight;
        public string CurrentSelectedCharacter { get; private set; }

        public bool IsActive => _eyeTrackingDomain.IsActive;

        public EyeTrackingContext(IEyeTrackingDomain eyeTrackingDomain)
        {
            _eyeTrackingDomain = eyeTrackingDomain;
        }

        public void SetIsActive( bool isValue )
        {
            _eyeTrackingDomain.SetIsActive( isValue );
        }

        public void SetCharacterID( string id )
        {
            CurrentSelectedCharacter = id;
        }
    }
}