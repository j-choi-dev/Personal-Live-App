using LiveAppCore;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LiveAppUI.View
{
    public abstract class ObservableFloatInput : MonoBehaviour
    {
        public abstract float Value { get; set; }
        public abstract bool Interactable { get; set; }

        public abstract IObservable<float> OnValueChanged { get; }
        public abstract void SetValueWithoutNotify( float value );

        public abstract void SetIndeterminateValue();

        private FloatValueRange _range = null;

        public void SetRestrictions( List<IValueRestriction> restrictions )
        {
            _range = restrictions.FirstOrDefault( rest => rest is FloatValueRange ) as FloatValueRange;
        }

        protected float ApplyRange( float value )
        {
            return _range != null ? _range.Apply( value ) : value;
        }
    }
}
