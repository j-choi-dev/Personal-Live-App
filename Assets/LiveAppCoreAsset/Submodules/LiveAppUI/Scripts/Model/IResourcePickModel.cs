using System;
using System.Collections.Generic;
using System.Text;
using UniRx;

namespace LiveAppUI.Model
{
    /// <summary>
    /// 별도의 Model로 만든 이유
    /// 피킹 과정에서 다른 model과 공유하는 데이터나 로직이 많이 없고
    /// 추후 현재 피킹한 리소스를 구독해서 각자 처리할 부분이 여러 곳에 생길 것 같은데
    /// 다른 model에 넣으면 해당 model의 불필요한 부분까지 접근하게 됨 @OH 26.11.03
    /// </summary>
    public interface IResourcePickModel
    {
        /// <summary>
        /// 현재 피킹으로 선택된 리소스의(type, id). 선택이 없으면 null
        /// 같은 튜플을 재선택 시 발행x
        /// </summary>
        IReadOnlyReactiveProperty<(ResourceType type, string id)?> Selected { get; }

        /// <summary>
        /// 화면 좌표의 리소스를 선택
        /// </summary>
        /// <param name="viewportX">렌더 카메라 기준 viewport X(0~1)</param>
        /// <param name="viewportY">렌더 카메라 기준 viewport Y(0~1)</param>
        void PickAt(float viewportX, float viewportY);

        /// <summary>
        /// 선택 해제
        /// </summary>
        void ClearSelection();
    }
}
