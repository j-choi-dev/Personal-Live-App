using LiveAppUI.Domain;
using System;
///ResourceSDK.Domain enum 가시성을 위해 rename
using DomainResourceType = StudioResourceSDK.Domain.ResourceType;

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

}
