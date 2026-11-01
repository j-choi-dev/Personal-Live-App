using Cysharp.Threading.Tasks;
using LiveApp.UI.Presenter;
using LiveAppUI.Model;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using Zenject;

namespace LiveAppUI.Presenter
{
    /// <summary>
    /// 리소스 메뉴 관련 View-Model의 Presenter
    /// </summary>
    public class ResourceMenuPresenter : MonoBehaviour
    {
        private IResourceMenuView _resourceMenuView;
        private IResourceListView _resourceListView;
        private INoticeModalView _noticeModalView;
        private IResourceRemoveView _resourceRemoveView;
        private IResourceListModel _resourceListModel;
        private ResourceType _currentResourceType;
        private ResourceType _currentRemoveFilter = ResourceType.None;
        private ServerType _currentResourceServerType;

        private List<string> _resources = new List<string>();
        private readonly Dictionary<string, (ResourceType type, string id)> _removeCellMap
            = new Dictionary<string, (ResourceType type, string id)>();

        [Inject]
        public void Initialize(
            IResourceMenuView resourceMenuView,
            IResourceListView resourceListView,
            IResourceRemoveView resourceRemoveView,
            INoticeModalView noticeModalView,
            IResourceListModel resourceListModel)
        {
            _resourceMenuView = resourceMenuView;
            _resourceListView = resourceListView;
            _noticeModalView = noticeModalView;
            _resourceRemoveView = resourceRemoveView;
            _resourceListModel = resourceListModel;
        }

        private void Awake()
        {
            _resourceListView.SetActive( false );
            _resourceRemoveView.SetActive(false);
            SubscribeView();
            SubscribeModel();
        }

        private void Start()
        {
            _currentResourceType = ResourceType.None;
            _currentResourceServerType = ServerType.Develop;
            _currentRemoveFilter = ResourceType.None;

            _resourceListView.SetServerItem(GetServerIndex(ServerType.Develop));

            _resourceRemoveView.SetFilterItem(GetResourceIndex(ResourceType.None));

            _resourceListModel.SetCurrentServerType( ResourceType.Character, ServerType.Develop);

            _resourceListModel.SetCurrentServerType( ResourceType.BackGround, ServerType.Develop);

            _resourceListModel.SetCurrentServerType( ResourceType.Prop, ServerType.Develop);
        }

        private void SubscribeView()
        {
            _resourceListView.OnClickClose
                .Subscribe(_ => CloseResourceListView())
                .AddTo(this);

            _resourceMenuView.OnClickAvatar
                .Subscribe(_ => UpdateResourceList( ResourceType.Character, _resourceListModel.GetCurrentServerType(ResourceType.Character)))
                .AddTo(this);

            _resourceMenuView.OnClickStage
                .Subscribe(_ => UpdateResourceList( ResourceType.BackGround, _resourceListModel.GetCurrentServerType(ResourceType.BackGround)))
                .AddTo(this);

            _resourceMenuView.OnClickProp
                .Subscribe(_ => UpdateResourceList(ResourceType.Prop, _resourceListModel.GetCurrentServerType(ResourceType.Prop)))
                .AddTo(this);

            _resourceMenuView.OnClickRemove
                .Subscribe(_ => UpdateRemoveList(_currentRemoveFilter) )
                .AddTo(this);

            _resourceListView.OnServerChange
                .Subscribe(index =>
                {
                    if (_resourceListModel.IsLoading)
                    {
                        var currentIndex =
                            GetServerIndex(_currentResourceServerType);

                        if (index != currentIndex)
                            _resourceListView.SetServerItem(currentIndex);

                        return;
                    }

                    var serverType = GetServerType(index);

                    if (serverType == _currentResourceServerType)
                        return;

                    UpdateResourceList(_currentResourceType, serverType);
                })
                .AddTo(this);

            _resourceListView.OnClickLoad
                .Subscribe(arg => _resourceListModel.LoadResourceProcess(_currentResourceType,
                    _currentResourceServerType,
                    _resourceListView.CurrentSelectedItemList.ToList()
                )
                .Forget())
                .AddTo(this);

            _resourceRemoveView.OnFilterChanged
                .Subscribe(index => UpdateRemoveList(GetResourceType(index)))
                .AddTo(this);

            _resourceRemoveView.OnClickRemove
                .Subscribe(_ => RemoveSelectedResources())
                .AddTo(this);

            _resourceRemoveView.OnClickClose
                .Subscribe(_ => CloseResourceRemoveView())
                .AddTo(this);


        }

        private void SubscribeModel()
        {
            _resourceListModel.OnCharacterListChanged
                .Subscribe(list =>
                {
                    _resourceListView.ClearList();
                    for( var i = 0; i < list.Count; i++ ) 
                    {
                        _resourceListView.AddListItem( list[i].id, list[i].displayName );
                    }
                } )
                .AddTo( this );
            _resourceListModel.OnBackStageListChanged
                .Subscribe(list =>
                {
                    _resourceListView.ClearList();
                    for (var i = 0; i < list.Count; i++)
                    {
                        _resourceListView.AddListItem(list[i].id, list[i].displayName);
                    }
                })
                .AddTo(this);
            _resourceListModel.OnPropListChanged
                .Subscribe(list =>
                {
                    _resourceListView.ClearList();
                    for (var i = 0; i < list.Count; i++)
                    {
                        _resourceListView.AddListItem(list[i].id, list[i].displayName);
                    }
                })
                .AddTo(this);

            _resourceListModel.OnSceneResourceListChanged
                .Subscribe(list => RefreshRemoveList(list))
                .AddTo(this);
        }

        private void UpdateResourceList( ResourceType resourceType, ServerType serverType)
        {
            UpdateResourceListAsync(resourceType, serverType).Forget(Debug.LogException);
        }

        private async UniTask UpdateResourceListAsync( ResourceType resourceType, ServerType serverType)
        {
            if (_resourceListModel.IsLoading)
            {
                return;
            }

            if (resourceType == ResourceType.None)
            {
                return;
            }

            var isSameResourceType = _currentResourceType == resourceType;
            var isSameServerType = _currentResourceServerType == serverType;

            if (isSameResourceType &&
                _resourceListView.IsActive &&
                isSameServerType)
            {
                CloseResourceListView();
                {
                    return;
                }
            }

            _currentResourceType = resourceType;
            _currentResourceServerType = serverType;

            _resourceListModel.SetCurrentServerType( resourceType, serverType);

            _resourceListView.SetTitle(resourceType.ToString());
            _resourceListView.SetActive(true);

            var loadTask = _resourceListModel.GetResourceList( resourceType, serverType);
            _resourceListView.SetServerItem(GetServerIndex(serverType));

            await loadTask;
        }
        private void UpdateRemoveList(ResourceType resourceType)
        {

            var isSameFilter = _currentRemoveFilter == resourceType;
            _currentRemoveFilter = resourceType;

            if (isSameFilter &&
                _resourceRemoveView.IsActive)
            {
                CloseResourceRemoveView();
                return;
            }


            _resourceRemoveView.SetActive(true);
            _resourceRemoveView.SetFilterItem(GetResourceIndex(resourceType));
            _resourceListModel.FilterRemoveResource(resourceType);
        }

        private void CloseResourceListView()
        {
            _resourceListView.SetActive( false );
        }

        private void CloseResourceRemoveView()
        {
            _resourceRemoveView.SetActive(false);
        }

        private void RefreshRemoveList(IReadOnlyList<(ResourceType type, string id, string displayName)> items)
        {
            if(_resourceRemoveView.IsActive == false)
            {
                return;
            }

            _resourceRemoveView.ClearList();
            _removeCellMap.Clear();

            for(var i = 0; i < items.Count; i++)
            {
                var cellId = $"{items[i].type}_{items[i].id}";
                _resourceRemoveView.AddListItem(cellId, items[i].displayName);
                _removeCellMap[cellId] = (items[i].type, items[i].id);
            }

        }

        private void RemoveSelectedResources()
        {
            var selected = _resourceRemoveView.CurrSelectedItemList;
            if(selected.Count == 0)
            {
                Debug.LogWarning("삭제할 리소스를 선택해주세요.");
                return;
            }

            var targets = new List<(ResourceType type, string id)>();
            for( var i = 0; i < selected.Count; i++)
            {
                if (_removeCellMap.TryGetValue(selected[i], out var target))
                {
                    targets.Add(target);
                }
            }

            if(targets.Count == 0)
            {
                return;
            }

            _resourceListModel.RemoveResourcesProcess(targets);
        }

        private int GetServerIndex( ServerType type ) 
            => ( int )(type - 1);
        private ServerType GetServerType( int index )
            => ( ServerType )(index + 1);

        private int GetResourceIndex(ResourceType type)
            => (int)type;
        private ResourceType GetResourceType(int index)
            => (ResourceType)index;
    }
}