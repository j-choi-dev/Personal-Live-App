using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using LiveAppCore;

namespace LiveAppUI.View
{
    public abstract class ObservableIntSlider : MonoBehaviour
    {
        public abstract int Value { get; set; }
        public abstract IObservable<int> OnValueChanged { get; }
        public abstract void SetValueWithoutNotify( int value );

        public abstract int MinValue { get; set; }
        public abstract int MaxValue { get; set; }
        public abstract void SetRange( int minValue, int maxValue );
        public abstract bool Interactable { get; set; }

        public abstract void SetIndeterminateValue();

        public void SetRestrictions( List<IValueRestriction> restrictions )
        {
            var valueRange = restrictions
                .FirstOrDefault( rest => rest is IntValueRange ) as IntValueRange;
            if( valueRange != null )
            {
                SetRange( valueRange.Min, valueRange.Max );
            }
        }
    }
}
