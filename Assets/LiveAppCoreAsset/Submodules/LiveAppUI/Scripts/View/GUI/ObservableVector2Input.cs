using UnityEngine;
using UniRx;

namespace LiveAppUI.View
{
    public class ObservableVector2Input : ObservableVector2
    {
        [SerializeField] private ObservableFloatInput X = null;
        [SerializeField] private ObservableFloatInput Y = null;

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
