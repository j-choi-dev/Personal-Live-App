using Cysharp.Threading.Tasks;
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
        private IResourceListModel _resourceListModel;

        private ResourceType _currentResourceType;
        private ServerType _currentResourceServerType;

        private List<string> _resources = new List<string>();

        [Inject]
        public void Initialize(
            IResourceMenuView resourceMenuView,
            IResourceListView resourceListView,
            IResourceListModel resourceListModel)
        {
            _resourceMenuView = resourceMenuView;
            _resourceListView = resourceListView;
            _resourceListModel = resourceListModel;
        }

        private void Awake()
        {
            _resourceListView.SetActive( false );
            SubscribeView();
            SubscribeModel();
        }

        private async void Start()
        {
            await _resourceListModel.InitializeServerConfig();
            _currentResourceType = ResourceType.None;
            _currentResourceServerType = ServerType.Develop;

            _resourceListView.SetServerItem( GetServerIndex( ServerType.Develop ) );
            _resourceListModel.SetCurrentServerType( ResourceType.Character, ServerType.Develop );
            _resourceListModel.SetCurrentServerType( ResourceType.Stage, ServerType.Develop );
            _resourceListModel.SetCurrentServerType( ResourceType.Prop, ServerType.Develop );
            await _resourceListModel.GetResourceList( ResourceType.Character, _currentResourceServerType );
        }

        private void SubscribeView()
        {
            _resourceListView.OnClickClose
                .Subscribe( _ => CloseResourceListView() )
                .AddTo( this );

            _resourceMenuView.OnClickAvatar
                .Subscribe( _ =>
                {
                    _currentResourceServerType = _resourceListModel.GetCurrentServerType( ResourceType.Character );
                    _resourceListView.SetServerItem( GetServerIndex( _currentResourceServerType ) );
                    UpdateResourceList(
                        ResourceType.Character,
                        _resourceListModel.GetCurrentServerType( ResourceType.Character ) );
                } )
                .AddTo( this );
            _resourceMenuView.OnClickStage
                .Subscribe( _ =>
                {
                    _currentResourceServerType = _resourceListModel.GetCurrentServerType( ResourceType.Stage );
                    _resourceListView.SetServerItem( GetServerIndex( _currentResourceServerType ) );
                    UpdateResourceList(
                        ResourceType.Stage,
                    _resourceListModel.GetCurrentServerType( ResourceType.Stage ) );
                } )
                .AddTo( this );
            _resourceMenuView.OnClickProp
                .Subscribe( _ =>
                {
                    _currentResourceServerType = _resourceListModel.GetCurrentServerType( ResourceType.Prop );
                    _resourceListView.SetServerItem( GetServerIndex( _currentResourceServerType ) );
                    UpdateResourceList(
                        ResourceType.Prop,
                        _resourceListModel.GetCurrentServerType( ResourceType.Prop ) );
                } )
                .AddTo( this );

            _resourceListView.OnServerChange
                .Subscribe( server =>
                {
                    UpdateResourceList( _currentResourceType, GetServerType( server ) );
                } )
                .AddTo( this );

            _resourceListView.OnClickLoad
                .Subscribe(arg => _resourceListModel.LoadResourceProcess( _currentResourceType, 
                    _currentResourceServerType, 
                    _resourceListView.CurrentSelectedItemList.ToList() 
                )
                .Forget() )
                .AddTo ( this );
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
        }

        private void UpdateResourceList( ResourceType resourceType, ServerType serverType )
        {
            var isSameResourceType = _currentResourceType == resourceType;
            var isSameServerType = _currentResourceServerType == serverType;
            if( isSameResourceType && _resourceListView.IsActive && isSameServerType )
            {
                CloseResourceListView();
                return;
            }
            _currentResourceType = resourceType; 
            _currentResourceServerType = serverType;
            _resourceListModel.SetCurrentServerType( _currentResourceType, _currentResourceServerType );
            _resourceListView.SetTitle( _currentResourceType.ToString() );
            _resourceListModel.GetResourceList( _currentResourceType, _currentResourceServerType ).Forget();
            _resourceListView.SetActive( true );
        }

        private void CloseResourceListView()
        {
            _resourceListView.SetActive( false );
        }

        private int GetServerIndex( ServerType type ) 
            => ( int )type - 1;
        private ServerType GetServerType( int index )
            => ( ServerType )( index + 1 );
    }
}