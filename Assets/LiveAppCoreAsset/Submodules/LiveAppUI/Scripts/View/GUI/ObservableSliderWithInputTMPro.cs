using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UniRx;

namespace LiveAppUI.View
{
    public class ObservableSliderWithInputTMPro : ObservableSlider
    {
        [SerializeField] private Slider _slider = null;
        [SerializeField] private TMP_InputField _input = null;
        [SerializeField] private Image _sliderHandleImage;
        [SerializeField] private int _numDigits;

        public override float Value
        {
            get { return _slider.value; }
            set
            {
                SetIndeterminate( false );
                _slider.value = value;
            }
        }

        public override IObservable<float> OnValueChanged => _slider.onValueChanged.AsObservable();

        public override float MinValue { get { return _slider.minValue; } set { _slider.minValue = value; } }
        public override float MaxValue { get { return _slider.maxValue; } set { _slider.maxValue = value; } }

        public override bool Interactable
        {
            get
            {
                return _slider.interactable;
            }
            set
            {
                _slider.interactable = value;
                _input.interactable = value;
            }
        }

        public override void SetRange( float minValue, float maxValue )
        {
            MinValue = minValue;
            MaxValue = maxValue;
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
            SetTextWithoutNotify( value );
        }

        private void SetSliderValue( string text )
        {
            if( float.TryParse( text, out var value ) )
            {
                _slider.value = value;
            }
        }

        private void SetTextWithoutNotify( float value )
        {
            if( _input.isFocused == false )
            {
                _input.SetTextWithoutNotify( value.ToString( $"F{_numDigits}" ) );
            }
        }

        public override void SetIndeterminateValue()
        {
            SetIndeterminate( true );
        }

        private void SetIndeterminate( bool indeterminate )
        {
            if( _sliderHandleImage != null )
            {
                var color = _sliderHandleImage.color;
                color.a = indeterminate ? 0f : 1f;
                _sliderHandleImage.color = color;
            }

            if( indeterminate )
            {
                _input.SetTextWithoutNotify( "---" );
            }
            else
            {
                SetTextWithoutNotify( Value );
            }
        }
    }
}
