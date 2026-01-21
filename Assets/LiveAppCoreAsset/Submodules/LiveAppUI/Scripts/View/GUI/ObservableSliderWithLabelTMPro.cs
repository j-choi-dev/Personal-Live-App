using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UniRx;

namespace LiveAppUI.View
{
    public class ObservableSliderWithLabelTMPro : ObservableSlider
    {
        [SerializeField] private Slider _slider = null;
        [SerializeField] private TMP_Text _text = null;
        [SerializeField] private Image _sliderHandleImage;

        public override float Value
        {
            get => _slider.value;
            set
            {
                SetIndeterminate( false );
                _slider.value = value;
            }
        }
        public override IObservable<float> OnValueChanged => _slider.onValueChanged.AsObservable();

        public override float MinValue { get => _slider.minValue; set => _slider.minValue = value; }
        public override float MaxValue { get => _slider.maxValue; set => _slider.maxValue = value; }
        public override bool Interactable { get => _slider.interactable; set => _slider.interactable = value; }

        public override void SetRange( float minValue, float maxValue )
        {
            _slider.minValue = minValue;
            _slider.maxValue = maxValue;
        }

        public override void SetRangeWithoutNotify( float minValue, float maxValue )
        {
            MinValue = float.MinValue;
            MaxValue = float.MaxValue;
            _slider.SetValueWithoutNotify( Mathf.Clamp( _slider.value, minValue, maxValue ) );
            MinValue = minValue;
            MaxValue = maxValue;
        }

        public override void SetValueWithoutNotify( float value )
        {
            SetIndeterminate( false );
            _slider.SetValueWithoutNotify( value );
        }

        public override void SetIndeterminateValue()
        {
            SetIndeterminate( true );
        }

        private void SetIndeterminate( bool indeterminate )
        {
            var color = _sliderHandleImage.color;
            color.a = indeterminate ? 0f : 1f;
            _sliderHandleImage.color = color;

            if( indeterminate )
            {
                _text.text = "---";
            }
        }
    }
}
