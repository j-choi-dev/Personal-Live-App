using UnityEngine;

namespace LiveAppUI.View
{
    public class ObservableVector2Slider : ObservableVector2
    {
        [SerializeField] private ObservableSlider X = null;
        [SerializeField] private ObservableSlider Y = null;

        public override bool Interactable
        {
            get
            {
                return X.Interactable;
            }
            set
            {
                X.Interactable = value;
                Y.Interactable = value;
            }
        }

        public override void SetValueWithoutNotify( Vector2 value )
        {
            m_Value = value;

            X.SetValueWithoutNotify( value.x );
            Y.SetValueWithoutNotify( value.y );
        }

        public override void SetIndeterminateValue()
        {
            X.SetIndeterminateValue();
            Y.SetIndeterminateValue();
        }
    }
}
