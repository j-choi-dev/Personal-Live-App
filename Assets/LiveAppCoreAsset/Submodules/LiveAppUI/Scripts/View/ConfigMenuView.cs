using LiveAppUI.Presenter;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace LiveAppUI.View
{
    public class ConfigMenuView : MonoBehaviour, IConfigMenuView
    {
        [SerializeField] private ButtonViewPair _obsViewPair = null;
        [SerializeField] private ButtonViewPair _youtubeViewPair = null;

        private int _selectedIndex = -1;

        public bool IsActive => throw new System.NotImplementedException();

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
