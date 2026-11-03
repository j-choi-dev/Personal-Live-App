using System;

namespace LiveAppUI.Presenter
{
    /// <summary>
    /// 클릭을 감지하여 viewport좌표로 반환(ex 방송 미리보기 RawImage)
    /// </summary>
    public interface IPlayAreaView
    {
        /// <summary>
        /// 미리보기 화면을 왼쪽 클릭 시 클릭 위치를 렌더 카메라의 viewport 좌표(0~1)로 발행(uv좌표)
        /// 이미지 바깥 클릭은 발행x
        /// </summary>
        IObservable<UnityEngine.Vector2> OnViewportClicked { get; } 
    }
}
