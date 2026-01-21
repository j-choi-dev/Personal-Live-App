using UnityEngine;
using LiveAppCore;

namespace LiveAppUI.View
{
    public class ObservableValueRangeSlider : ObservableValueRange
    {
        [SerializeField] private ObservableSlider _min = null;
        [SerializeField] private ObservableSlider _max = null;

        public override bool Interactable
        {
            get
            {
                return _min.Interactable;
            }
            set
            {
                _min.Interactable = value;
                _max.Interactable = value;
            }
        }

        public override void SetValueWithoutNotify( ValueRange value )
        {
            _value = value;

            _min.SetValueWithoutNotify( value.min );
            _max.SetValueWithoutNotify( value.max );
        }

        public override void SetIndeterminateValue()
        {
            _min.SetIndeterminateValue();
            _max.SetIndeterminateValue();
        }

        public void SetRangeSliderRange(ValueRange range)
        {
            _min.SetRange(range.min, range.max);
            _max.SetRange(range.min, range.max);
        }

        public void SetRangeSliderRangeWithoutNotify( ValueRange range )
        {
            _min.SetRangeWithoutNotify( range.min, range.max );
            _max.SetRangeWithoutNotify( range.min, range.max );
        }
    }
}
