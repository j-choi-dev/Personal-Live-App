using LiveAppUI.Presenter;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace LiveAppUI.View
{
    public class ConfigMenuView : MonoBehaviour, IConfigMenuView
    {
        [SerializeField] protected ObservableButton _closeButton = null;
        [SerializeField] protected ObservableButton _cancleButton = null;
        [SerializeField] private ButtonViewPair _obsViewPair = null;
        [SerializeField] private ButtonViewPair _youtubeViewPair = null;

        private int _selectedIndex = -1;

        public IObservable<Unit> OnClickClose => _closeButton.OnClick;

        public IObservable<Unit> OnClickCancle => _cancleButton.OnClick;

        public bool IsActive => gameObject.activeSelf;

        private void Awake()
        {
            _obsViewPair.button.OnClick
                .Subscribe( arg =>
                {
                    _obsViewPair.view.SetActive(true);
                    _youtubeViewPair.view.SetActive(false);
                } )
                .AddTo( this );
            _youtubeViewPair.button.OnClick
                .Subscribe( arg =>
                {
                    _obsViewPair.view.SetActive( false );
                    _youtubeViewPair.view.SetActive( true );
                } )
                .AddTo( this );
        }

        private void Start()
        {
            _obsViewPair.view.SetActive( true );
            _youtubeViewPair.view.SetActive( false );
        }

        public void SetActive( bool isActive )
        {
            gameObject.SetActive( isActive );
        }
    }
}
