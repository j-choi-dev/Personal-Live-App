using Cysharp.Threading.Tasks;
using LiveAppUI.Model;
using LiveAppUI.View;
using System;
using UniRx;
using UnityEngine;
using Zenject;

namespace LiveAppUI.Presenter
{
    public class ResourceMenuPresenter : MonoBehaviour
    {
        private IResourceMenuView _resourceMenuView;
        private IResourceListView _resourceListView;
        private IResourceListModel _resourceListModel;

        private ResourceType _currentResourceType = ResourceType.None;
        private ServerType _currentResourceServerType = ServerType.None;

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

        private void SubscribeView()
        {
            _resourceListView.OnClickClose
                .Subscribe( _ => CloseResourceListView() )
                .AddTo( this );

            _resourceMenuView.OnClickAvatar
                .Subscribe( _ => UpdateResourceList(
                    ResourceType.Character,
                    _resourceListModel.GetCurrentServerType( ResourceType.Character ) )
                )
                .AddTo( this );
            _resourceMenuView.OnClickStage
                .Subscribe( _ => UpdateResourceList(
                    ResourceType.Stage,
                    _resourceListModel.GetCurrentServerType( ResourceType.Stage ) )
                )
                .AddTo( this );
            _resourceMenuView.OnClickProp
                .Subscribe( _ => UpdateResourceList(
                    ResourceType.Prop,
                    _resourceListModel.GetCurrentServerType( ResourceType.Prop ) )
                )
                .AddTo( this );

            _resourceListView.OnServerChange
                .Subscribe( server =>
                {
                    _resourceListModel.SetCurrentServerType( _currentResourceType, GetServerType( server ) );
                    UpdateResourceList( _currentResourceType, GetServerType( server ) );
                } )
                .AddTo( this );
        }

        private void SubscribeModel()
        {
            _resourceListModel.OnCharacterListChanged
                .Subscribe(list =>
                {
                    _resourceListView.ResetList();
                    for( var i = 0; i < list.Count; i++ ) 
                    {
                        _resourceListView.AddListItem( list[i].id, list[i].displayName );
                    }
                } )
                .AddTo( this );
        }

        private async void Start()
        {
            await _resourceListModel.InitializeServerConfig();
            _currentResourceType = ResourceType.Character;
            _currentResourceServerType = ServerType.Develop;

            _resourceListView.SetServerItem( GetServerIndex( ServerType.Develop ) );
            _resourceListModel.SetCurrentServerType( _currentResourceType, _currentResourceServerType );
        }

        private void UpdateResourceList( ResourceType resourceType, ServerType serverType )
        {
            var isSameResourceType = _currentResourceType == resourceType;
            if( isSameResourceType && _resourceListView.IsActive )
            {
                CloseResourceListView();
                return;
            }
            var currentServer = _resourceListModel.GetCurrentServerType( resourceType );
            _resourceListView.SetServerItem( (int)currentServer-1 );

            switch( resourceType )
            {
                case ResourceType.Character:
                    ShowAvatarResource();
                    break;

                case ResourceType.Stage:
                    ShowStageResource();
                    break;

                case ResourceType.Prop:
                    ShowPropResource();
                    break;

                default:
                    CloseResourceListView();
                    break;
            }
        }

        private void ShowAvatarResource()
        {
            _resourceListView.SetTitle( _currentResourceType.ToString() );
            _resourceListModel.GetResourceList( _currentResourceType, _currentResourceServerType ).Forget();

            // TODO 추후 예시 @Choi 26.06.01
            // var resources = _resourceModel.GetCharacterResources();
            // _resourceListView.SetResources( resources );

            _resourceListView.SetActive( true );
        }

        private void ShowStageResource()
        {
            _resourceListView.SetTitle( ResourceType.Stage.ToString() );

            // TODO 추후 예시 @Choi 26.06.01
            // var resources = _resourceModel.GetStageResources();
            // _resourceListView.SetResources( resources );

            _resourceListView.SetActive( true );
        }

        private void ShowPropResource()
        {
            _resourceListView.SetTitle( ResourceType.Prop.ToString() );

            // TODO 추후 예시 @Choi 26.06.01
            // var resources = _resourceModel.GetPropResources();
            // _resourceListView.SetResources( resources );

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