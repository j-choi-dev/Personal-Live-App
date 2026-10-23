using System;
using LiveAppCore;
using LiveAppUI.Domain;

namespace LiveAppUI.Presenter
{
    /// <summary>
    /// Inspector Area에서 transform에 대한 view
    /// slider를 (속성,축)형태로 찾아서 표시하고 사용자 입력을 (속성, 축, 값)형태로 알림
    /// </summary>
    public interface ITransformInspectorView
    {
        /// <summary>
        /// slider 조작으로 값 변경 시 발행
        /// </summary>
        IObservable<(InspectorTransformProperty prop, InspectorTransformAxis axis, float value)> OnValueChanged { get; }

        /// <summary>
        /// 생성 및 초기값 할당 시에는 이벤트 발생 안 함
        /// </summary>
        void ShowChannel(InspectorTransformProperty prop, InspectorTransformAxis axis, ValueRange range, float value);
        void ShowEmpty();
    }
}
