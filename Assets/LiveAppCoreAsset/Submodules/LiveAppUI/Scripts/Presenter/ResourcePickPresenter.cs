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
        private IPlayAreaView _view;
        private IResourcePickModel _pickmodel;

        [Inject]
        public void Initialize(
            IPlayAreaView view,
            IResourcePickModel pickmodel)
        {
            _view = view;
            _pickmodel = pickmodel;

            SubscribeView();
        }

        private void SubscribeView()
        {
            _view.OnViewportClicked
                .Subscribe(uv => _pickmodel.PickAt(uv.x, uv.y))
                .AddTo(this);
        }
     

    }
}
