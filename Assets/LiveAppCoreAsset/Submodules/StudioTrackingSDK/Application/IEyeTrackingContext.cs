using System;

namespace StudioTrackingSDK.Application
{
    public interface IEyeTrackingContext
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
        
        string CurrentSelectedCharacter { get; }

        /// <summary>
        /// Active 상태
        /// </summary>
        bool IsActive { get; }

        void SetCharacterID( string id );

        /// <summary>
        /// Active 여부
        /// </summary>
        void SetIsActive( bool isValue );
    }
}
