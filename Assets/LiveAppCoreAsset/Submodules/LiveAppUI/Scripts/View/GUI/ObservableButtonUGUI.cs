using UnityEngine;
using UnityEngine.UI;
using System;
using UniRx;

namespace LiveAppUI.View
{
	public class ObservableButtonUGUI : ObservableButton
	{
		[SerializeField] private Button _button = null;

		public override IObservable<Unit> OnClick => _button.OnClickAsObservable();
		public override bool Interactable { get => _button.interactable; set => _button.interactable = value; }

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
	}
}
