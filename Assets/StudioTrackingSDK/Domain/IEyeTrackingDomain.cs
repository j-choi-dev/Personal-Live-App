using System;

namespace StudioTrackingSDK.Domain
{
    public interface IEyeTrackingDomain
    {
        /// <summary>
        /// EyeBall Angle X
        /// </summary>
        IObservable<float> OnEyeBallAngleX { get; }
        /// <summary>
        /// EyeBall Angle Y
        /// </summary>
        IObservable<float> OnEyeBallAngleY { get; }

        IObservable<float> OnEyeBlinkLeft { get; }
        IObservable<float> OnEyeBlinkRight { get; }

        /// <summary>
        /// Active 상태
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Active 여부
        /// </summary>
        void SetIsActive( bool isValue );
    }
}
