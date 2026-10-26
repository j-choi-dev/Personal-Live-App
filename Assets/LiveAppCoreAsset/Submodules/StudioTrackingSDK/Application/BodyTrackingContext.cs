using StudioCharacterSDK.Domain;
using StudioTrackingSDK.Domain;
using System;

namespace StudioTrackingSDK.Application
{
    public class BodyTrackingContext : IBodyTrackingContext
    {
        private IBodyTrackingDomain _bodyTrackingDomain;

        public IObservable<float> OnBodyAngleX => _bodyTrackingDomain.OnBodyAngleXChanged;

        public IObservable<float> OnBodyAngleY => _bodyTrackingDomain.OnBodyAngleYChanged;

        public IObservable<float> OnBodyAngleZ => _bodyTrackingDomain.OnBodyAngleZChanged;

        public bool IsActive => _bodyTrackingDomain.IsActive;

        public string CurrentSelectedCharacter { get; private set; }

        public void SetCharacterID(string id)
        {
            CurrentSelectedCharacter = id;
        }

        public void SetIsActive(bool isValue)
        {
            _bodyTrackingDomain.SetIsActive(isValue);
        }
    }
}
