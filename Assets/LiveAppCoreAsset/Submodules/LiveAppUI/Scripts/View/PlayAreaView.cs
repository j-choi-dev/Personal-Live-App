using LiveAppUI.Presenter;
using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace LiveAppUI.View
{
    public class PlayAreaView : MonoBehaviour, IPlayAreaView
    {
        /// <summary>
        /// RenderTexture(RT_OBS_Output)를 표시하는 RawImage
        /// </summary>
        [SerializeField] private RawImage _rawImage;
        [SerializeField] private bool _logClick; //테스트용 클릭 로그 presenter 연결 후 제거 예정 @OH 26.11.03
        private IObservable<Vector2> _onViewportClicked;
        public IObservable<Vector2> OnViewportClicked => _onViewportClicked ??= CreateViewportClickStream();

        private void Start()
        {
            Debug.Log($"[PlayAreaView] Start | logClick: {_logClick} | rawImage: {(_rawImage != null ? _rawImage.name : "null")}", this);
            if (_logClick)
            {
                OnViewportClicked
                    .Subscribe(uv => Debug.Log($"[PlayAreaView] Viewport 클릭 : {uv.x:F3}, {uv.y:F3}"))
                    .AddTo(this);
            }
        }
  
        /// <summary>
        /// 클릭한 데이터의 스트림을 가공할 파이프라인 반환
        /// </summary>
        private IObservable<Vector2> CreateViewportClickStream()
        {
            //변환 파이프라인
            return _rawImage.OnPointerClickAsObservable()
                .Do(e => Debug.Log($"[PlayAreaView] 클릭 수신: {e.button}, {e.position}"))
                .Where(e => e.button == PointerEventData.InputButton.Left)
                .Select(e => ToViewportPoint(e.position, e.pressEventCamera))
                .Do(p => Debug.Log($"[PlayAreaView] 변환 결과: {p}"))
                .Where(point => point.HasValue)
                .Select(point => point.Value);
        }

        /// <summary>
        /// 스크린 좌표 -> RawImage 로컬 -> 0~1(uv좌표계)정규화 -> uvRect 보정 순서로 viewport 좌표를 구함
        /// </summary>
        /// <param name="screenPos">클릭한 스크린 좌표</param>
        /// <param name="evnetCamera">Canvas가 Overlay면 null</param>
        /// <returns>렌더 카메라 기준 viewport 좌표(0~1). 이미지 바깥이면 null</returns>
        private Vector2? ToViewportPoint(Vector2 screenPos, Camera eventCamera)
        {
            RectTransform rectTransform = _rawImage.rectTransform;

            //RawImage가 이동, 확대, 회전되어 있어도 로컬 좌표로 정확히 변환됨
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle
              (rectTransform, screenPos, eventCamera, out Vector2 local) == false)
            {
                return null;
            }

            Rect rect = rectTransform.rect;
            if (rect.width <= 0.0f || rect.height <= 0.0f)
            {
                return null;
            }

            //pivot과 무관하게 rect기준으로 0~1 정규화
            var normalized = new Vector2((local.x - rect.xMin) / rect.width,
                                         (local.y - rect.yMin) / rect.height);

            // uvRect로 보정 전에 범위 검증을 해야 텍스쳐 일부만 표시할 때도 정확함
            if(normalized.x < 0.0f || normalized.x > 1.0f
            || normalized.y < 0.0f || normalized.y > 1.0f)
            {
                return null;
            }

            // RawImage가 텍스쳐의 어느 부분을 보여주는지(uvRect)반영. 기본값(0,0,1,1)이면 그대로
            Rect uvRect = _rawImage.uvRect;
            return new Vector2(uvRect.x + normalized.x * uvRect.width,
                               uvRect.y + normalized.y * uvRect.height);
        }

    }
}
