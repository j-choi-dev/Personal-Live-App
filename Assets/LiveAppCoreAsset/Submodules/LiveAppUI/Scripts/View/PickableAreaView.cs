using LiveAppUI.Presenter;
using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace LiveAppUI.View
{
    public class PickableAreaView : MonoBehaviour, IPickableAreaView
    {
        /// <summary>
        /// 플레이 화면 내에 클릭 감지해서 피킹 가능한 영역
        /// </summary>
        [SerializeField] private  Image _pickableArea;
        /// <summary>
        /// 선택 표시 UI
        /// </summary>
        [SerializeField] private RectTransform _highlight;
        private IObservable<Vector2> _onViewportClicked;
        public IObservable<Vector2> OnViewportClicked => _onViewportClicked ??= CreateViewportClickStream();
  
        /// <summary>
        /// 클릭한 데이터의 스트림을 가공할 파이프라인 반환
        /// </summary>
        private IObservable<Vector2> CreateViewportClickStream()
        {
            //변환 파이프라인
            return _pickableArea.OnPointerClickAsObservable()
                .Where(e => e.button == PointerEventData.InputButton.Left)
                .Select(e => ToViewportPoint(e.position, e.pressEventCamera))
                .Where(point => point.HasValue)
                .Select(point => point.Value);
        }

        /// <summary>
        /// 스크린 좌표 -> pickableArea 로컬 좌표 -> (0~1)정규화 순서로 viewport 좌표를 구함
        /// </summary>
        /// <param name="screenPos">클릭한 스크린 좌표</param>
        /// <param name="evnetCamera">Canvas가 Overlay면 null</param>
        /// <returns>렌더 카메라 기준 viewport 좌표(0~1). 이미지 바깥이면 null</returns>
        /// 현재 RT_OBS_Output을 표시하는 rawImage의 display 영역과 같다는 전제하임 @OH 26.11.06
        private Vector2? ToViewportPoint(Vector2 screenPos, Camera eventCamera)
        {
            RectTransform rectTransform = _pickableArea.rectTransform;

            //_pickableArea이 이동, 확대, 회전되어 있어도 로컬 좌표로 정확히 변환됨
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

            //pivot과 무관하게 rect기준으로 0~1 정규화 -> 이 값이 곧 viewport 좌표
            var viewport = new Vector2((local.x - rect.xMin) / rect.width,
                                       (local.y - rect.yMin) / rect.height);

            if(viewport.x < 0.0f || viewport.x > 1.0f
            || viewport.y < 0.0f || viewport.y > 1.0f)
            {
                return null;
            }

            return viewport;
        }

        //-------------------------------------------------------------------implement

        public void ShowHighlight(float minX, float minY, float maxX, float maxY)
        {
            if(_highlight == null)
            {
                return;
            }

            _highlight.anchorMin = new Vector2(minX, minY);
            _highlight.anchorMax = new Vector2(maxX, maxY);
            _highlight.offsetMin = Vector2.zero;
            _highlight.offsetMax = Vector2.zero;

            if(_highlight.gameObject.activeSelf == false)
            {
                _highlight.gameObject.SetActive(true);
            }
        }

        public void HideHighlight()
        {
            if(_highlight == null)
            {
                return;
            }

            if(_highlight.gameObject.activeSelf)
            {
                _highlight.gameObject.SetActive(false);
            }
        }

        //------------------------------------------------------------------/implement

    }
}
