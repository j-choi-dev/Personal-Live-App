using LiveAppCore;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LiveAppUI.View
{
    public abstract class ObservableIntInput : MonoBehaviour
    {
        public abstract int Value { get; set; }
        public abstract bool Interactable { get; set; }

        public abstract IObservable<int> OnValueChanged { get; }
        public abstract void SetValueWithoutNotify( int value );
        public abstract void SetIndeterminateValue();

        private IntValueRange _range = null;
        public void SetRestrictions( List<IValueRestriction> restrictions )
        {
            _range = restrictions.FirstOrDefault( rest => rest is IntValueRange ) as IntValueRange;
        }

        protected int ApplyRange( int value )
        {
            return _range != null ? _range.Apply( value ) : value; 
        }
    }
}
