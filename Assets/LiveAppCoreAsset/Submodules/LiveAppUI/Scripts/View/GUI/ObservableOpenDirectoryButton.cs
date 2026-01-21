using LiveAppCore.Domain;
using System;
using UniRx;
using UnityEngine;
using Zenject;

namespace LiveAppUI.View
{
	public class ObservableOpenDirectoryButton : MonoBehaviour
	{
		[SerializeField] private ObservableButton _button;
		[SerializeField] private ObservableInput _pathInput;
		[SerializeField] private string _title;

		public IObservable<string> OnPathSelected => _onPathSelected;
		private Subject<string> _onPathSelected = new Subject<string>();
		public string Path => _pathInput.Text;

		private IFileBrowser _browser;

		[Inject]
		public void Initialize( IFileBrowser browser )
		{
			_browser = browser;
		}

		private void Awake()
		{
			_pathInput.OnEndEdit
				.Subscribe(_onPathSelected.OnNext)
				.AddTo(this);
			_button.OnClick.Subscribe( async _ =>
			{
				var path = await _browser.GetFolderPath( _title, "" );
				if( string.IsNullOrEmpty( path ) == false )
				{
					_pathInput.Text = path;
					_onPathSelected.OnNext(_pathInput.Text);
				}
			} )
			.AddTo( this );
		}

		private bool _interactable;
		public bool Interactable
		{
			get => _interactable;
			set
			{
				_interactable = value;
				_pathInput.Interactable = value;
				_button.Interactable = value;
			}
		}
	}
}
