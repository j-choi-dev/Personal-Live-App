using LiveAppUI.Domain;
using System;

namespace LiveAppUI.Model
{
    /// <summary>
    /// UI에서 선택한 리소스오브젝트를 기억하고 그 데이터를 가져와서 UI변환해주는 model
    /// </summary>
    public interface ITransformInspectorModel
    {
        /// <summary>
        /// 대상이 없는 상태로 바뀔 땐 Empty반환
        /// </summary>
        IObservable<TransformInspectorData> OnTargetChanged { get; }
        /// <summary>
        /// Inspector 대상 리소스오브젝트 지정
        /// </summary>
        void SetTarget(ResourceType type, string id);
        void ClearTarget();
        /// <summary>
        /// Inspector 대상 리소스오브젝트 값 변경. 대상 없으면 무시
        /// 대상은 model이 기억하니 setvalue시 대상은 안 보내도 됨
        /// </summary>
        void SetValue(InspectorTransformProperty prop, InspectorTransformAxis axis, float value);
    }
}
