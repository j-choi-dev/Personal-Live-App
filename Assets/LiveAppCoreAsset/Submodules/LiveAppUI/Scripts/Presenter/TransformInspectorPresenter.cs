using Cysharp.Threading.Tasks;
using LiveAppUI.Domain;
using LiveAppUI.Model;
using UniRx;
using UnityEngine;
using Zenject;

namespace LiveAppUI.Presenter
{
    /// <summary>
    /// TransformInspectorView <-> TransformInspectorModel presenter
    /// </summary>
    public class TransformInspectorPresenter : MonoBehaviour
    {
        ITransformInspectorView _view;
        ITransformInspectorModel _model;

        [Inject]
        public void Initialize(
            ITransformInspectorView view,
            ITransformInspectorModel model)
        {
            _view = view;
            _model = model;
        }

        public void Awake()
        {
            SubscribeView();
            SubscribeModel();
        }

        private void SubscribeView()
        {
            
            _view.OnValueChanged
                .Subscribe(data => _model.SetValue(data.prop, data.axis, data.value))
                .AddTo(this);
        }

        private void SubscribeModel()
        {
            _model.OnTargetChanged
                .Subscribe(Render)
                .AddTo(this);
        }

        private void Render(TransformInspectorData data)
        {
            _view.ShowEmpty();

            foreach(TransformInspectorChannelData channel in data._channels)
            {
                Debug.Log($"[InspectorPresenter] Render {channel.Property}.{channel.Axis} 범위={channel.Range.min}~{channel.Range.max}, 값={channel.Value}");
                _view.ShowChannel(channel.Property, channel.Axis, channel.Range, channel.Value);
            }
        }

    }
}
