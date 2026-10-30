using System;
using System.Collections.Generic;
using System.Text;
using LiveAppUI.Domain;
///ResourceSDK.Domain enum 가시성을 위해 rename
using DomainResourceType = StudioResourceSDK.Domain.ResourceType;
using DomainTransformProperty = StudioResourceSDK.Domain.TransformProperty;
using DomainTransformAxis = StudioResourceSDK.Domain.TransformAxis;
using UnityEditor;

namespace LiveAppUI.Model
{
    /// <summary>
    /// UI의 ResourceType enum <-> StudioResourceSDK.Domain.ResourceType으로 변환 해주는 유틸 클래스
    /// </summary>
    public static class ResourceTypeConvertExtensions
    {
        /// <summary>
        /// UI ResourceType => ResourceSDK.Domain.ResourceType
        /// </summary>
        public static DomainResourceType ToDomain(this ResourceType type)
        {
            switch (type)
            {
                case ResourceType.None:
                    return DomainResourceType.None;
                case ResourceType.Character:
                    return DomainResourceType.Character;
                case ResourceType.BackGround:
                    return DomainResourceType.BackGround;
                case ResourceType.Prop:
                    return DomainResourceType.Prop;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        /// <summary>
        /// ResourceSDK.Domain.ResourceType => UI ResourceTypeResourceSDK.Domain.ResourceType
        /// </summary>
        public static ResourceType ToLiveAppUI(this DomainResourceType type)
        {
            switch (type)
            {
                case DomainResourceType.None:
                    return ResourceType.None;
                case DomainResourceType.Character:
                    return ResourceType.Character;
                case DomainResourceType.BackGround:
                    return ResourceType.BackGround;
                case DomainResourceType.Prop:
                    return ResourceType.Prop;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }

    /// <summary>
    /// InspectorUI용  Tranform enum <-> StudioResourceSDK.Domain의 Transform enum 컨버터 유틸 클래스
    /// </summary>
    public static class InspectorTransformConvertExtensions
    {
        /// <summary>
        /// UI TransformProperty => ResourceSDK.Domain.TramsformProperty
        /// </summary>
        public static DomainTransformProperty ToDomain(this InspectorTransformProperty prop)
        {
            switch (prop)
            {
                case InspectorTransformProperty.Position:
                    return DomainTransformProperty.Position;
                case InspectorTransformProperty.Rotation:
                    return DomainTransformProperty.Rotation;
                case InspectorTransformProperty.Scale:
                    return DomainTransformProperty.Scale;
                default:
                    throw new ArgumentOutOfRangeException(nameof(prop), prop, null);
            }
        }

        /// <summary>
        /// ResourceSDK.Domain.TramsformPropertyUI TransformProperty => UI TransformProperty
        /// </summary>
        public static InspectorTransformProperty ToInspector( this DomainTransformProperty prop)
        {
            switch(prop)
            {
                case DomainTransformProperty.Position:
                    return InspectorTransformProperty.Position;
                case DomainTransformProperty.Rotation:
                    return InspectorTransformProperty.Rotation;
                case DomainTransformProperty.Scale:
                    return InspectorTransformProperty.Scale;
                default:
                    throw new ArgumentOutOfRangeException(nameof(prop), prop, null);
            }
        }

        /// <summary>
        /// UI TransformAxis => ResourceSDK.Domain.TramsformAxis
        /// </summary>
        public static DomainTransformAxis ToDomain( this InspectorTransformAxis axis)
        {
            switch (axis)
            {
                case InspectorTransformAxis.X:
                    return DomainTransformAxis.X;
                case InspectorTransformAxis.Y:
                    return DomainTransformAxis.Y;
                case InspectorTransformAxis.Z:
                    return DomainTransformAxis.Z;
                case InspectorTransformAxis.Uniform:
                    return DomainTransformAxis.Uniform;
                default:
                    throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
            }
        }

        /// <summary>
        /// ResourceSDK.Domain.TramsformAxisUI TransformAxis => UI TransformAxis
        /// </summary>
        public static InspectorTransformAxis ToInspector(this DomainTransformAxis axis)
        {
            switch (axis)
            {
                case DomainTransformAxis.X:
                    return InspectorTransformAxis.X;
                case DomainTransformAxis.Y:
                    return InspectorTransformAxis.Y;
                case DomainTransformAxis.Z:
                    return InspectorTransformAxis.Z;
                case DomainTransformAxis.Uniform:
                    return InspectorTransformAxis.Uniform;
                default:
                    throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
            }
        }
    }
}
