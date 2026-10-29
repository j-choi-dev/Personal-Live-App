using LiveAppCore;
using System;
using System.Collections.Generic;

namespace LiveAppUI.Domain
{
    /// <summary>
    /// StudioResourceSDK.Domain의 TransformProperty과 매핑되는 UI용 enum
    /// </summary>
    public enum InspectorTransformProperty
    {
        Position,
        Rotation,
        Scale,
    }
    /// <summary>
    /// StudioResourceSDK.Domain의 TransformAxis과 매핑되는 UI용 enum
    /// </summary>
    public enum InspectorTransformAxis
    {
        X,
        Y,
        Z,
        Uniform, // 비율 고정
    }

    /// <summary>
    /// Inspector에 표시할 항목 하나에 대한 data ex) X축 슬라이더 하나에 대한 데이터
    /// </summary>
    public class TransformInspectorChannelData
    {
        public InspectorTransformProperty Property { get; }
        public InspectorTransformAxis Axis { get; }
        public ValueRange Range { get; }
        public float Value { get; }

        public TransformInspectorChannelData(
               InspectorTransformProperty prop,
               InspectorTransformAxis axis,
               ValueRange range,
               float value )
        {
            Property = prop;
            Axis = axis;
            Range = range;
            Value = value;
        }
    }

    /// <summary>
    /// Inspector View에 표시 될 전체 데이터
    /// </summary>
    public class TransformInspectorData
    {
        public static readonly TransformInspectorData Empty =
            new TransformInspectorData(false, Array.Empty<TransformInspectorChannelData>());
    
        public bool HasTarget { get; }
        public IReadOnlyList<TransformInspectorChannelData> _channels { get; }

        public TransformInspectorData(bool hasTarget, IReadOnlyList<TransformInspectorChannelData> channels)
        {
            HasTarget = hasTarget;
            _channels = channels;
        }

    }
}
