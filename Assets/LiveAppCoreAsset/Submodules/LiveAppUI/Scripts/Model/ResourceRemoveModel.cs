using System;
using System.Collections.Generic;
using System.Text;
using UniRx;
using StudioResourceSDK.Domain;
using StudioResourceSDK.Application;
using Unity.VisualScripting;
using System.Linq;

namespace LiveAppUI.Model
{
    public class ResourceRemoveModel : IResourceRemoveModel, IDisposable
    {
        private ISceneResourceListContext _sceneResourceListContext;
        private CompositeDisposable _disposables = new CompositeDisposable();
        private Subject<IReadOnlyList<(ResourceType type, string id, string displayName)>> _onSceneResourceListChanged
            = new Subject<IReadOnlyList<(ResourceType type, string id, string displayName)>>();
        public IObservable<IReadOnlyList<(ResourceType type, string id, string displayName)>> OnSceneResourceListChanged
            => _onSceneResourceListChanged;
        private ResourceType _filter = ResourceType.None;
        //여러 개 삭제 시 목록 갱신은 마지막에 한 번만 하기 위한 플래그
        private bool _suppressRefresh = false;

        public ResourceRemoveModel(ISceneResourceListContext sceneResourceListContext)
        {
            _sceneResourceListContext = sceneResourceListContext;

            _sceneResourceListContext.OnChangedCharacterList
                .Where(_ => !_suppressRefresh)
                .Subscribe(_ => Refresh())
                .AddTo(_disposables);

            _sceneResourceListContext.OnChangedBackGroundList
                .Where(_ => !_suppressRefresh)
                .Subscribe(_ => Refresh())
                .AddTo(_disposables);

            _sceneResourceListContext.OnChangedPropList
                .Where(_ => !_suppressRefresh)
                .Subscribe(_ => Refresh())
                .AddTo(_disposables);
        }

        public void RemoveResourcesFromScene(IReadOnlyList<(ResourceType type, string id)> targets)
        {
            _suppressRefresh = true;

            try
            {
                for(var i = 0; i < targets.Count; i++)
                {
                    _sceneResourceListContext.RemoveResource(ConvertResourceType(targets[i].type), targets[i].id);
                }
            }
            finally
            {
                _suppressRefresh = false;
            }

            Refresh();
        }

        public void FilterRemoveResource(ResourceType resourceType)
        {
            _filter = resourceType;
            Refresh();
        }
        private bool IsVisible(ResourceType type)
            => _filter == ResourceType.None || _filter == type;

        private void Refresh()
        {
            var list = new List<(ResourceType type, string id, string displayName)>();

            if (IsVisible(ResourceType.Character))
            {
                list.AddRange(_sceneResourceListContext.CharacterList
                    .Select(c => (ResourceType.Character, c.ID, c.ID)));
            }

            if(IsVisible(ResourceType.BackGround))
            {
                list.AddRange(_sceneResourceListContext.BackGroundList
                    .Select(b => (ResourceType.BackGround, b.ID, b.ID)));
            }

            if(IsVisible(ResourceType.Prop))
            {
                list.AddRange(_sceneResourceListContext.PropList
                    .Select(p => (ResourceType.Prop, p.ID, p.ID)));
            }

            _onSceneResourceListChanged.OnNext(list);
        }
        private StudioResourceSDK.Domain.ResourceType ConvertResourceType(ResourceType type)
            => (StudioResourceSDK.Domain.ResourceType)Enum.Parse(
                typeof(StudioResourceSDK.Domain.ResourceType),
                type.ToString());

        public void Dispose()
        {
            
        }
    }
}
