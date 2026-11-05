using StudioResourceSDK.Application;
using System;
using UniRx;
//Domain 전체 using하면 ResourceType 쪽이 헷갈려서 별도로 재정의 @OH 26.11.05
using ViewportRect = StudioResourceSDK.Domain.ViewportRect;
using ViewportPoint = StudioResourceSDK.Domain.ViewportPoint;

namespace LiveAppUI.Model
{
    public class ResourcePickModel : IResourcePickModel, IDisposable
    {
        private readonly IResourcePickContext _pickContext;
        private readonly ISceneResourceListContext _listContext;
        private readonly ReactiveProperty<(ResourceType type, string id)?> _selected
            = new ReactiveProperty<(ResourceType type, string id)?>(null);
        public IReadOnlyReactiveProperty<(ResourceType type, string id)?> Selected => _selected;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        public ResourcePickModel(
            IResourcePickContext pickContext,
            ISceneResourceListContext listContext)
        {
            _pickContext = pickContext;
            _listContext = listContext;

            _listContext.OnChangedCharacterList
                .Subscribe(_ => ClearSelectionIfRemoved())
                .AddTo(_disposables);
        }

        //----------------------------------------------------------------implement
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

        public bool TryGetSelectedViewportRect(out float minX, out float minY, out float maxX, out float maxY)
        {
            minX = minY = maxX = maxY = 0.0f;

            if(_selected.Value.HasValue == false)
            {
                return false;
            }

            //_selected.Value - ReactiveProperty의 현재 값(nullable 튜플)
            //_selected.Value.Value - nullable안의 실제 튜플의 값
            var selected = _selected.Value.Value;

            //UI ResourceType -> ResourceSDK.Domain ResourceType
            if(_pickContext.TryGetViewportRect(selected.type.ToDomain(), selected.id, out ViewportRect rect) == false)
            {
                return false;
            }

            minX = rect.MinX;
            minY = rect.MinY;
            maxX = rect.MaxX;
            maxY = rect.MaxY;

            return true;
        }

        public void ClearSelection()
        {
            _selected.Value = null;

        }
        //---------------------------------------------------------------/implement

        /// <summary>
        /// 선택한 리소스가 씬 목록에서 사라지면 선택 해제
        /// </summary>
        private void ClearSelectionIfRemoved()
        {
            if (_selected.Value.HasValue == false)
            {
                return;
            }

            //_selected.Value - ReactiveProperty의 현재 값(nullable 튜플)
            //_selected.Value.Value - nullable안의 실제 튜플의 값
            var selected = _selected.Value.Value;

            // UI ResourceType → Domain ResourceType
            if (_listContext.IsExist(selected.type.ToDomain(), selected.id) == false)
            {
                ClearSelection();
            }
        }

        public void Dispose()
        {
            _disposables.Dispose();
            _selected.Dispose();
        }
    }
}
