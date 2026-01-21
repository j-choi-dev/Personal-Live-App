using UnityEngine;
using System;
using UniRx;

namespace LiveAppUI.View
{
    public abstract class ObservableVector2 : MonoBehaviour
    {
        protected Vector2 m_Value = new Vector2();
        public Vector2 Value
        {
            get => m_Value;
            set { SetValueWithoutNotify( value ); m_ValueChanged.OnNext( value ); }
        }

        protected Subject<Vector2> m_ValueChanged = new Subject<Vector2>();
        public IObservable<Vector2> OnValueChanged => m_ValueChanged;

        public abstract bool Interactable { get; set; }
        public abstract void SetValueWithoutNotify( Vector2 value );

        public abstract void SetIndeterminateValue();
    }
}
