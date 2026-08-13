using Live2D.Cubism.Core;

namespace StudioCharacterSDK.Domain
{
    public enum AvatarPartsParameter
    {
        FaceAngle_X,
        FaceAngle_Y,
        FaceAngle_Z,
        EyeBallX,
        EyeBallY,
        LeftEyeBlink,
        RightEyeBlink,
        BodyAngle_X,
        BodyAngle_Y,
        BodyAngle_Z,
        MouthForm,
        MouthOpen_Y,
    }

    [System.Serializable]
    public class AvatarParameterPairData
    {
        public AvatarPartsParameter paramterType;
        public CubismParameter parameter;
    }
}
