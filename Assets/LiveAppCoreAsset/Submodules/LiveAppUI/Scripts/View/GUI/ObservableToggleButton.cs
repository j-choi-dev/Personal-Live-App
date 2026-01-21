using UnityEngine;
using UnityEngine.UI;
using System;
using UniRx;

namespace LiveAppUI.View
{
	public class ObservableToggleButton : ObservableButton
	{
		[SerializeField] private Button _button = null;
        [SerializeField] private Image _active = null;
        [SerializeField] private Image _inactive = null;

        public bool IsActive { get; private set; } = false;

        private Subject<bool> _onActiveChange = new Subject<bool>();
        public IObservable<bool> OnActiveChange => _onActiveChange;

        public override IObservable<Unit> OnClick => _button.OnClickAsObservable();

        public override bool Interactable { get => _button.interactable; set => _button.interactable = value; }

        private void Awake()
        {
            _button.OnClickAsObservable()
                .Subscribe( isOn =>
                {
                    IsActive = !IsActive;
                    ApplyActive();
                } );
        }

        public override string Name
		{
			get { return _button.name; }
			set { _button.name = value; }
		}

		public override Sprite Sprite
		{
			get { return _button.image.sprite; }
			set { _button.image.sprite = value; }
		}

        private void ApplyActive()
        {
            if ( IsActive )
            {
                _active.gameObject.SetActive( true );
                _inactive.gameObject.SetActive( false );

            }
            else
            {
                _active.gameObject.SetActive( false );
                _inactive.gameObject.SetActive( true );
            }
            _onActiveChange.OnNext( IsActive );
        }
	}
}
