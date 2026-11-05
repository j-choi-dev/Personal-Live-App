using LiveAppUI.Model;
using StudioResourceSDK.Application;
using ViewportPoint = StudioResourceSDK.Domain.ViewportPoint;
using System;
using System.Collections.Generic;
using System.Text;
using UniRx;

namespace LiveAppUI.Model
{
    public class ResourcePickModel : IResourcePickModel, IDisposable
    {
        private readonly IResourcePickContext _pickContext;
        private readonly ReactiveProperty<(ResourceType type, string id)?> _selected
            = new ReactiveProperty<(ResourceType type, string id)?>(null);
        public IReadOnlyReactiveProperty<(ResourceType type, string id)?> Selected => _selected;

        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        public ResourcePickModel(IResourcePickContext pickContext)
        {
            _pickContext = pickContext;

            _selected
                .Subscribe(OnSelectedChagned)
                .AddTo(_disposables);
        }

        //----------------------------implement------------------------------start
        public void PickAt(float viewportX, float viewportY)
        {
            var result = _pickContext.Pick(new ViewportPoint(viewportX, viewportY));

            if(result.HasValue == false)
            {
                _selected.Value = null;
                return;
            }

            // ResourceSDK.Domain.ResourceType -> LiveAppUI ResourceType
            _selected.Value = (result.Value.type.ToLiveAppUI(), result.Value.id);
        }

        public void ClearSelection()
        {
            _selected.Value = null;
        
        }
        //----------------------------implement------------------------------end

        private void OnSelectedChagned( ( ResourceType type, string id )? selected)
        {
            if(selected.HasValue)
            {
                _pickContext.ShowHighlight(selected.Value.type.ToDomain(), selected.Value.id);
            }
            else
            {
                _pickContext.HideHighlight();
            }
        }
        public void Dispose()
        {
            _disposables.Dispose();
            _selected.Dispose();
        }
    }
}
