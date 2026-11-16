using StudioResourceSDK.Domain;
using System;
using System.Collections.Generic;

namespace StudioResourceSDK.Application
{
    /// <summary>
    /// Transform Inspector 유스케이스
    /// 리소스 타입 별로 조절 가능한 항목과 범위를 정하고 범위 안의 값만 적용
    /// </summary>
    /// <remarks>값은 모두 기준 상태에서의 오프셋(원래 상태 = 0)</remarks>
    public interface ITransformInspectorContext
    {
        /// <summary>
        /// 로드된 모든 리소스의 오프셋 변경 이벤트
        /// 구독하는 쪽에서 Type, Id로 필요한 대상만 걸러서 사용
        /// </summary>
        IObservable<TransformOffsetChangedData> OnOffsetChanged { get; }

        /// <returns>type에 맞는 채널 반환 없으면 빈 채널 반환</returns>
        IReadOnlyList<TransformChannelInfo> GetChannels(ResourceType type);

        /// <returns>대상이 없거나 조절 가능한 항목이 아니면 false</returns>
        bool TryGetValue(ResourceType type, string id, TransformProperty prop, TransformAxis axis, out float value);
        /// <returns> 대상이 없거나 조절 가능한 항목이 아니면 false</returns>
        bool SetValue(ResourceType type, string id, TransformProperty prop, TransformAxis axis, float value);

    }
}
