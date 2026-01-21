using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UniRx;

namespace LiveAppUI.View
{
    public class ObservableIntSliderWithInputTMPro : ObservableIntSlider
    {
        [SerializeField] private Slider _slider = null;
        [SerializeField] private TMP_InputField _input = null;
        [SerializeField] private Image _sliderHandleImage;

        public override int Value
        {
            get { return ( int )_slider.value; }
            set
            {
                SetIndeterminate( false );
                _slider.value = value;
            }
        }
        public override IObservable<int> OnValueChanged => _slider.onValueChanged
            .AsObservable()
            .Select( fValue => ( int )fValue );

        public override int MinValue { get { return (int)_slider.minValue; } set { _slider.minValue = value; } }
        public override int MaxValue { get { return ( int )_slider.maxValue; } set { _slider.maxValue = value; } }

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

        public override void SetRange( int minValue, int maxValue )
        {
            MinValue = minValue;
            MaxValue = maxValue;
        }

        public override void SetValueWithoutNotify( int value )
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

        private void SetTextWithoutNotify( int value )
        {
            if( _input.isFocused == false )
            {
                _input.SetTextWithoutNotify( value.ToString() );
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

            _input.SetTextWithoutNotify( "---" );
        }
    }
}
