using LiveAppUI.Presenter;
using UnityEngine;
using UniRx;
using System;

namespace LiveAppUI.View
{
    public class ResourceMenuView : MonoBehaviour, IResourceMenuView
    {
        [SerializeField] private ObservableButton _avatarButton = null;
        [SerializeField] private ObservableButton _stageButton = null;
        [SerializeField] private ObservableButton _propButton = null;
        [SerializeField] private ObservableButton _backButton = null;
        public IObservable<Unit> OnBackButtonClick => _backButton.OnClick;

        public bool IsActive => gameObject.activeSelf;

        public IObservable<Unit> OnClickAvatar => _avatarButton.OnClick;

        public IObservable<Unit> OnClickStage => _stageButton.OnClick;

        public IObservable<Unit> OnClickProp => _propButton.OnClick;

        private void Awake()
        {
            _backButton.OnClick
                .Subscribe( _ =>
                {
                    gameObject.SetActive( false );
                } )
                .AddTo( this );
        }

        public void SetActive( bool isActive )
        {
            gameObject.SetActive( isActive );
        }
    }
}
