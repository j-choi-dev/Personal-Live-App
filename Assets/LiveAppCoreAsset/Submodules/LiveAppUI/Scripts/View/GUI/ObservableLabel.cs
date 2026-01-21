using System;
using UniRx;
using UnityEngine;

namespace LiveAppUI.View
{
    public abstract class ObservableLabel : MonoBehaviour
    {
        public abstract string Text { get; set; }
        public bool Interactable { get; set; }
        public string Value { get => Text; set => Text = value; }

        public IObservable<string> OnValueChanged { get; } = Observable.Never<string>();

        public void SetIndeterminateValue()
        {
            Text = "---";
        }

        public void SetValueWithoutNotify( string value )
        {
            Text = value;
        }

        public abstract void SetAlignment( TextAlignment alignment );
    }
}
