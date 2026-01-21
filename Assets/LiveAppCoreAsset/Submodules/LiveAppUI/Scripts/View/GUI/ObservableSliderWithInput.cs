using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using LiveAppCore;

namespace HoloStudioV2UI.View
{
    public abstract class ObservableSliderWithInput : MonoBehaviour
    {
        public abstract float Value { get; set; }
        public abstract IObservable<float> OnValueChanged { get; }
        public abstract void SetValueWithoutNotify( float value );

        public abstract float MinValue { get; set; }
        public abstract float MaxValue { get; set; }
        public abstract bool Interactable { get; set; }

        public abstract void SetRange( float minValue, float maxValue );
        public abstract void SetIndeterminateValue();

        public void SetRestrictions( List<IValueRestriction> restrictions )
        {
            var valueRange = restrictions
                .FirstOrDefault( rest => rest is FloatValueRange ) as FloatValueRange;
            if( valueRange != null )
            {
                SetRange( valueRange.Min, valueRange.Max );
            }
        }
    }
}
