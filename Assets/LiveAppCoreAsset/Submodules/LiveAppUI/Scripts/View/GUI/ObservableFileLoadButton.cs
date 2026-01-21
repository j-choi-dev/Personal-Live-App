using LiveAppCore.Domain;
using System;
using UniRx;
using UnityEngine;
using Zenject;

namespace LiveAppUI.View
{
    public class ObservableFileLoadButton : MonoBehaviour
    {
        [SerializeField] private ObservableButton _button;
        [SerializeField] private string[] _extensions;
        [SerializeField] private string _title;

        private Subject<string> m_OnPathSelected = new Subject<string>();
        public IObservable<string> OnPathSelected => m_OnPathSelected;

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
                var path = await m_Browser.GetLoadPath( _title, "", _extensions );
                if( string.IsNullOrEmpty( path ) == false )
                {
                    m_OnPathSelected.OnNext( path );
                }
            } )
            .AddTo( this );
        }
    }
}
