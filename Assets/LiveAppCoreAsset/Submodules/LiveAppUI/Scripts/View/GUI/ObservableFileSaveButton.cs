using LiveAppCore.Domain;
using System;
using UniRx;
using UnityEngine;
using Zenject;

namespace LiveAppUI.View
{
    public class ObservableFileSaveButton : MonoBehaviour
    {
        [SerializeField] private ObservableButton _button;
        [SerializeField] private string[] _extensions;
        [SerializeField] private string _title;

        private Subject<string> _onPathSelected = new Subject<string>();
        public IObservable<string> OnPathSelected => _onPathSelected;

        public string Title { get => _title; set => _title = value; }
        public string[] Extensions { get => _extensions; set => _extensions = value; }

        private IFileBrowser m_Browser;

        [Inject]
        public void Initialize( IFileBrowser browser )
        {
            m_Browser = browser;
        }

        private void Awake()
        {
            _button.OnClick.Subscribe( async _ =>
            {
                var path = await m_Browser.GetSavePath( _title, "", _extensions );
                if( string.IsNullOrEmpty( path ) == false )
                {
                    _onPathSelected.OnNext( path );
                }
            } )
            .AddTo( this );
        }
    }
}
