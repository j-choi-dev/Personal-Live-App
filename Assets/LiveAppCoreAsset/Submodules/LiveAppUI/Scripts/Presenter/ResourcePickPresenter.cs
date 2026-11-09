using LiveAppUI.Model;
using UniRx;
using UnityEngine;
using Zenject;

namespace LiveAppUI.Presenter
{
    /// <summary>
    /// 플레이 화면 클릭(PlayAreaView) <-> PickModel presenter
    /// </summary>
    public class ResourcePickPresenter : MonoBehaviour
    {
        private IPickableAreaView _view;
        private IResourcePickModel _pickmodel;

        [Inject]
        public void Initialize(
            IPickableAreaView view,
            IResourcePickModel pickmodel)
        {
            _view = view;
            _pickmodel = pickmodel;

            SubscribeView();
            SubscribeModel();
        }

        private void SubscribeView()
        {
            _view.OnViewportClicked
                .Subscribe(uv => _pickmodel.PickAt(uv.x, uv.y))
                .AddTo(this);
        }

        private void SubscribeModel()
        {
            // 선택 중일 때만 매 프레임 갱신, 해제되면 한 번 숨긴 뒤 갱신x
            _pickmodel.Selected
                .Select
                (selected => selected.HasValue ?
                    Observable.EveryLateUpdate().Select(_ => Unit.Default) //선택 중 -> 매 프레임 신호 /EveryLateUpdate가 long을 발행해서 unit으로 변환
                  : Observable.Return(Unit.Default)                        //해제    -> 신호 한 번만 보내고 끝 -> UpdateHighlight에서 숨김 처리
                )
                .Switch() //가장 최근의 스트림만 구독하고 이전 스트림은 자동으로 해제
                .Subscribe(_ => UpdateHighlight())
                .AddTo(this);
        }
     
        private void UpdateHighlight()
        {
            if(_pickmodel.TryGetSelectedViewportRect(out float minX, out float minY, out float maxX, out float maxY) )
            {
                _view.ShowHighlight(minX, minY, maxX, maxY);
            }
            else
            {
                _view.HideHighlight();
            }
        }

    }
}
