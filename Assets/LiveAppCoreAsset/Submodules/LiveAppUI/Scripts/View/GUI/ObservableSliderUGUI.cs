using UnityEngine;
using UnityEngine.UI;
using System;
using UniRx;

namespace LiveAppUI.View
{
    public class ObservableSliderUGUI : ObservableSlider
    {
        [SerializeField] Slider m_Slider = null;
        [SerializeField] Image m_HandleImage;

        public override float Value
        {
            get => m_Slider.value;
            set
            {
                ShowHandle( true );
                m_Slider.value = value;
            }
        }
        public override IObservable<float> OnValueChanged => m_Slider.onValueChanged.AsObservable();

        public override float MinValue { get { return m_Slider.minValue; } set { m_Slider.minValue = value; } }
        public override float MaxValue { get { return m_Slider.maxValue; } set { m_Slider.maxValue = value; } }

        public override bool Interactable { get { return m_Slider.interactable; } set { m_Slider.interactable = value; } }

        public override void SetRange( float minValue, float maxValue )
        {
            MinValue = minValue;
            MaxValue = maxValue;
        }

        public override void SetRangeWithoutNotify( float minValue, float maxValue )
        {
            MinValue = float.MinValue;
            MaxValue = float.MaxValue;
            m_Slider.SetValueWithoutNotify( Mathf.Clamp( m_Slider.value, minValue, maxValue ) );
            MinValue = minValue;
            MaxValue = maxValue;
        }

        public override void SetValueWithoutNotify( float value )
        {
            ShowHandle( true );
            m_Slider.SetValueWithoutNotify( value );
        }

        public override void SetIndeterminateValue()
        {
            ShowHandle( false );
        }

        private void ShowHandle( bool show )
        {
            var color = m_HandleImage.color;
            color.a = show ? 1f : 0f;
            m_HandleImage.color = color;
        }
    }
}
