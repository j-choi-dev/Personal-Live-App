using System;

namespace StudioCharacterSDK.Domain
{
    public interface IBodyData
    {
        float BodyAngleX { get; }
        float BodyAngleY { get; }
        float BodyAngleZ { get; }

        IObservable<float> OnChangeBodyAngleX { get; }
        IObservable<float> OnChangeBodyAngleY { get; }
        IObservable<float> OnChangeBodyAngleZ { get; }


        void SetBodyAngleX(float value);
        void SetBodyAngleY(float value);
        void SetBodyAngleZ(float value);
    }
}
