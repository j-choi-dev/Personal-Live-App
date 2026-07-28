using System;

namespace StudioCharacterSDK.Domain
{
    public interface IFacialData
    {
        float FaceAngleX { get; }
        float FaceAngleY { get; }
        float FaceAngleZ { get; }

        IObservable<float> OnChangeFaceAngleX { get; }
        IObservable<float> OnChangeFaceAngleY { get; }
        IObservable<float> OnChangeFaceAngleZ { get; }

        void SetFaceAngleX( float value );
        void SetFaceAngleY( float value );
        void SetFaceAngleZ( float value );
    }
}
