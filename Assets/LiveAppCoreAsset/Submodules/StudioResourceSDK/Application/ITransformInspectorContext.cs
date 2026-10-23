using StudioResourceSDK.Domain;
using System.Collections.Generic;

namespace StudioResourceSDK.Application
{
    /// <summary>
    /// Transform Inspector 유스케이스
    /// 리소스 타입 별로 조절 가능한 항목과 범위를 정하고 범위 안의 값만 적용
    /// </summary>
    public interface ITransformInspectorContext
    {
        /// <summary>
        /// type에서 조절 가능한 항목 목록 없으면 빈 list
        IReadOnlyList<TransformChannelInfo> GetChannels(ResourceType type);
        /// <returns>대상이 없거나 조절 가능한 항목이 아니면 false</returns>
        bool TryGetValue(ResourceType type, string id, TransformProperty prop, TransformAxis axis, out float value);
        /// <returns> 대상이 없거나 조절 가능한 항목이 아니면 false</returns>
        bool SetValue(ResourceType type, string id, TransformProperty prop, TransformAxis axis, float value);

    }
}
