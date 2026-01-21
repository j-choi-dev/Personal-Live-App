using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using LiveAppCore;

namespace LiveAppUI.View
{
    public abstract class ObservableSlider : MonoBehaviour
    {
        public abstract float Value { get; set; }
        public abstract IObservable<float> OnValueChanged { get; }
        public abstract void SetValueWithoutNotify( float value );

        public abstract float MinValue { get; set; }
        public abstract float MaxValue { get; set; }
        public abstract void SetRange( float minValue, float maxValue );
        /// <summary>
        /// Range를 지정
        /// 지정된 값이 범위 밖일 경우, 범위 내로 Clamp됨
        /// </summary>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        public abstract void SetRangeWithoutNotify( float minValue, float maxValue );

        public abstract void SetIndeterminateValue();

        public void SetRestrictions( List<IValueRestriction> restrictions )
        {
            var range = restrictions.FirstOrDefault(rest => rest is FloatValueRange) as FloatValueRange;
            if( range != null )
            {
                SetRange( range.Min, range.Max );
            }
        }

        public abstract bool Interactable { get; set; }
    }
}
