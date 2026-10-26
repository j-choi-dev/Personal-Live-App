using System;

namespace StudioTrackingSDK.Application
{
    public interface IBodyTrackingContext
    {
        /// <summary>
        /// Body Angle X
        /// </summary>
        IObservable<float> OnBodyAngleX { get; }
        /// <summary>
        /// Body Angle Y
        /// </summary>
        IObservable<float> OnBodyAngleY { get; }
        /// <summary>
        /// Body Angle Z
        /// </summary>
        IObservable<float> OnBodyAngleZ { get; }

        string CurrentSelectedCharacter { get; }

        /// <summary>
        /// Active 상태
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Active 여부
        /// </summary>
        void SetIsActive( bool isValue );
        void SetCharacterID( string id );
    }
}
