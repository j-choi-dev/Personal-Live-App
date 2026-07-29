using System;

namespace StudioCharacterSDK.Domain
{
    public interface IFacialData
    {
        float FaceAngleX { get; }
        float FaceAngleY { get; }
        float FaceAngleZ { get; }


        float EyeBallAngleX { get; }
        float EyeBallAngleY { get; }
        float EyeBlinkLeft { get; }
        float EyeBlinkRight { get; }

        IObservable<float> OnChangeFaceAngleX { get; }
        IObservable<float> OnChangeFaceAngleY { get; }
        IObservable<float> OnChangeFaceAngleZ { get; }


        IObservable<float> OnChangeEyeBallAngleX { get; }
        IObservable<float> OnChangeEyeBallAngleY { get; }

        IObservable<float> OnChangeEyeBlinkLeft { get; }
        IObservable<float> OnChangeEyeBlinkRight { get; }

        void SetFaceAngleX( float value );
        void SetFaceAngleY( float value );
        void SetFaceAngleZ( float value );

        void SetEyeBallAngleX( float value );
        void SetEyeBallAngleY( float value );
        void SetEyeBlinkLeft( float value );
        void SetEyeBlinkRight( float value );
    }
}
