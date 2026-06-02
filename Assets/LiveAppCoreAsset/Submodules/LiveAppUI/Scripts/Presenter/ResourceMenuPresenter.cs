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

        private ResourceType _currentResourceType = ResourceType.None;

        [Inject]
        public void Initialize(
            IResourceMenuView resourceMenuView,
            IResourceListView resourceListView )
        {
            _resourceMenuView = resourceMenuView;
            _resourceListView = resourceListView;
        }

        private void Awake()
        {
            _resourceListView.SetActive( false );

            _resourceListView.OnClickClose
                .Subscribe( _ => CloseResourceList() )
                .AddTo( this );

            BindResourceButton( _resourceMenuView.OnClickAvatar, ResourceType.Character );
            BindResourceButton( _resourceMenuView.OnClickStage, ResourceType.Stage );
            BindResourceButton( _resourceMenuView.OnClickProp, ResourceType.Prop );
        }

        private void BindResourceButton(
            IObservable<Unit> onClick,
            ResourceType resourceType )
        {
            onClick
                .Subscribe( _ => ToggleResourceList( resourceType ) )
                .AddTo( this );
        }

        private void ToggleResourceList( ResourceType resourceType )
        {
            bool isSameResourceType = _currentResourceType == resourceType;

            if( isSameResourceType && _resourceListView.IsActive )
            {
                CloseResourceList();
                return;
            }

            ShowResourceList( resourceType );
        }

        private void ShowResourceList( ResourceType resourceType )
        {
            _currentResourceType = resourceType;

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
                    CloseResourceList();
                    break;
            }
        }

        private void ShowAvatarResource()
        {
            _resourceListView.SetTitle( ResourceType.Character.ToString() );

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

        private void CloseResourceList()
        {
            _resourceListView.SetActive( false );
        }
    }
}