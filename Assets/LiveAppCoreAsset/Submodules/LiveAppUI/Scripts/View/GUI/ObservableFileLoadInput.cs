using UniRx;
using UnityEngine;
using LiveAppCore.Domain;
using System;
using Zenject;
using Cysharp.Threading.Tasks;
using UnityEngine.Serialization;

namespace LiveAppUI.View
{
    public class ObservableFileLoadInput : MonoBehaviour
    {
        [SerializeField] private ObservableButton _browseButton;
        [SerializeField] private ObservableInput _input;
        [SerializeField] private ObservableButton _loadButton = null;

        [SerializeField] private string _title;
        [SerializeField] private string[] _extensions;
        [SerializeField] private bool _isMultiSelectMode = false;

        private IFileBrowser _fileBrowser;

        private Subject<string> _onLoadButtonClicked = new Subject<string>();
        public IObservable<string> OnLoadButtonClicked => _onLoadButtonClicked;

        public IObservable<string[]> OnLoadButtonClickedAtMultiSelectMode => onLoadButtonClickedAtMultiSelectMode;
        private Subject<string[]> onLoadButtonClickedAtMultiSelectMode = new();

        public string Title { get => _title; set => _title = value; }
        public string[] Extensions { get => _extensions; set => _extensions = value; }
        public bool IsMultiSelectMode { get => _isMultiSelectMode; set => _isMultiSelectMode = value; }

        public string Path
        {
            get => paths[0];
            set => paths[0] = _input.Text = value;
        }

        public string[] PathList
        {
            get => paths;
            set
            {
                paths = value;
                _input.Text = value.Length switch
                {
                    0 => string.Empty,
                    1 => value[0],
                    _ => $"{value[0]}, and more...",
                };
            }
        }

        private string[] paths = new string[1];

        [Inject]
        public void Initialize( IFileBrowser fileBrowser )
        {
            _fileBrowser = fileBrowser;
        }

        private void Awake()
        {
            _browseButton.OnClick
                .Subscribe( async _ => await BrowseFile() )
                .AddTo( this );

            _loadButton.OnClick
                .Subscribe( _ =>
                {
                    if ( _isMultiSelectMode )
                    {
                        onLoadButtonClickedAtMultiSelectMode.OnNext( PathList );
                    }
                    else
                    {
                        _onLoadButtonClicked.OnNext( Path );
                    }
                } )
                .AddTo( this );

            _loadButton.Interactable = false;
            _input.OnValueChanged
                .Subscribe( text => _loadButton.Interactable = !string.IsNullOrEmpty( text ) )
                .AddTo( this );
        }

        public void SetPathWithoutNotify( string path )
        {
            _input.SetTextWithoutNotify( path );
        }

        private async UniTask BrowseFile()
        {
            if( _isMultiSelectMode )
            {
                var paths = await _fileBrowser.GetLoadPathList( _title, _input.Text, _extensions );
                if ( paths.Length > 0 )
                {
                    PathList = paths;
                }
            }
            else
            {
                var path = await _fileBrowser.GetLoadPath( _title, _input.Text, _extensions );
                if ( string.IsNullOrEmpty( path ) == false )
                {
                    Path = path;
                }
            }
        }
    }
}
