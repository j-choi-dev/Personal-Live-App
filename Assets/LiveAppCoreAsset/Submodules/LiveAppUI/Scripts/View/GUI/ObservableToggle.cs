using UnityEngine;
using System;

namespace LiveAppUI.View
{
    public abstract class ObservableToggle : MonoBehaviour
    {
        public abstract bool IsActive { get; set; }
        public abstract IObservable<bool> OnActiveChanged { get; }
        public abstract bool Interactable { get; set; }
        public bool Value { get => IsActive; set => IsActive = value; }

        public IObservable<bool> OnValueChanged => OnActiveChanged;

        public abstract void SetIndeterminateValue();

        public abstract void SetIsActiveWithoutNotify( bool isActive );

        public void SetValueWithoutNotify( bool value )
            => SetIsActiveWithoutNotify( value );
    }
}
