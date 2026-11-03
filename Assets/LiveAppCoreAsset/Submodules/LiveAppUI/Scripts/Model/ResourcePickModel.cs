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

        public ResourcePickModel(IResourcePickContext pickContext)
        {
            _pickContext = pickContext;
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

        public void Dispose()
        {
            _selected.Dispose();
        }
    }
}
