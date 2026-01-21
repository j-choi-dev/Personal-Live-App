using System;
using UniRx;
using UnityEngine;
using LiveAppCore;

namespace LiveAppUI.View
{
    public abstract class ObservableValueRange : MonoBehaviour
    {
        protected ValueRange _value = new ValueRange();
        public ValueRange Value
        {
            get => _value;
            set { SetValueWithoutNotify( value ); m_ValueChanged.OnNext( value ); }
        }

        protected Subject<ValueRange> m_ValueChanged = new Subject<ValueRange>();
        public IObservable<ValueRange> OnValueChanged => m_ValueChanged;

        public abstract bool Interactable { get; set; }
        public abstract void SetValueWithoutNotify( ValueRange value );

        public abstract void SetIndeterminateValue();
    }
}
