using System;

namespace StudioTrackingSDK.Domain
{
    public interface IFaceTrackingDomain
    {
        /// <summary>
        /// Face Angle X
        /// </summary>
        IObservable<float> OnFaceAngleX { get; }
        /// <summary>
        /// Face Angle Y
        /// </summary>
        IObservable<float> OnFaceAngleY { get; }
        /// <summary>
        /// Face Angle Z
        /// </summary>
        IObservable<float> OnFaceAngleZ { get; }

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
