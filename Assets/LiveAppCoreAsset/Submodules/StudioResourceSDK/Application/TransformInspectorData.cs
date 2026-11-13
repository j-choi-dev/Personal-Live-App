using StudioResourceSDK.Domain;

namespace StudioResourceSDK.Application
{
    /// <summary>
    /// 리소스 하나의 Transform 오프셋이 바뀌었을 때 발행하는 데이터
    /// 구독하는 쪽은 Type, Id로 필요한 대상만 걸러서 사용
    /// </summary>
    public readonly struct TransformOffsetChangedData
    {
        public readonly ResourceType Type;
        public readonly string Id;
        public readonly TransformProperty Property;
        public readonly TransformAxis Axis;
        public readonly float Offset;

        public TransformOffsetChangedData(
            ResourceType type, 
            string id, 
            TransformProperty prop,
            TransformAxis axis, 
            float offset )
        {
            Type = type;
            Id = id;
            Property = prop;
            Axis = axis;
            Offset = offset;
        }
    }

    /// <summary>
    /// TransformInspectorContext 내부 기준 상태로 Context전용 데이터 <see cref="TransformInspectorContext"/>
    /// </summary>
    /// <remarks>한 번 기록한 뒤 바꾸지 않으므로 readonly.
    /// 다시 기록해야 한다면 Capture로 새로 만들어서 교체할 것.</remarks>
    public readonly struct TransformBase
    {
        public readonly float PositionX, PositionY, PositionZ;
        public readonly float RotationX, RotationY, RotationZ;
        public readonly float ScaleX, ScaleY, ScaleZ;

        public TransformBase(
            float posX, float posY, float posZ,
            float rotX, float rotY, float rotZ,
            float scX,  float scY,  float scZ )
        {
            PositionX = posX;   PositionY = posY;   PositionZ = posZ;
            RotationX = rotX;   RotationY = rotY;   RotationZ = rotZ;
            ScaleX = scX;   ScaleY = scY;   ScaleZ = scZ;
        }

        public static TransformBase Capture(ITransformData data)
        {
            return new TransformBase(
                data.PositionX, data.PositionY, data.PositionZ,
                data.RotationX, data.RotationY, data.RotationZ,
                data.ScaleX, data.ScaleY, data.ScaleZ);
        }

        /// <summary>속성, 축에 해당하는 기준값 반환</summary>
        /// <remarks>Scale Uniform은 X축을 대표로 사용</remarks>
        public float Get(TransformProperty prop, TransformAxis axis)
        {
            //Prop switch
            switch (prop)
            {
                case TransformProperty.Position:
                    switch (axis)
                    {
                        case TransformAxis.X: return PositionX;
                        case TransformAxis.Y: return PositionY;
                        case TransformAxis.Z: return PositionZ;
                    }
                    break;

                case TransformProperty.Rotation:
                    switch (axis)
                    {
                        case TransformAxis.X: return RotationX;
                        case TransformAxis.Y: return RotationY;
                        case TransformAxis.Z: return RotationZ;
                    }
                    break;

                case TransformProperty.Scale:
                    switch (axis)
                    {
                        case TransformAxis.X:
                        case TransformAxis.Uniform: return ScaleX;
                        case TransformAxis.Y: return ScaleY;
                        case TransformAxis.Z: return ScaleZ;
                    }
                    break;
            }

            return 0.0f;
        }

    }


}
