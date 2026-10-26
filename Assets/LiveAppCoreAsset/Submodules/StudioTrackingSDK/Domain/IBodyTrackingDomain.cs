using StudioCharacterSDK.Domain;
using System;

namespace StudioTrackingSDK.Domain
{
    public interface IBodyTrackingDomain
    {
        IObservable<float> OnBodyAngleXChanged { get; }
        IObservable<float> OnBodyAngleYChanged { get; }
        IObservable<float> OnBodyAngleZChanged { get; }
        bool IsActive { get; }
        void SetIsActive(bool isValue);
    }
}
