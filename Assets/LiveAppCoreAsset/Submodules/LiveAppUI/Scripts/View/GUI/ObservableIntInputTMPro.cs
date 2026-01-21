using UnityEngine;
using TMPro;
using System;
using UniRx;

namespace LiveAppUI.View
{
    public class ObservableIntInputTMPro : ObservableIntInput
    {
        [SerializeField] private TMP_InputField m_Input = null;

        public override IObservable<int> OnValueChanged => m_Input.onEndEdit.AsObservable()
            .Select( text => SafeParse( text ) );

        public override int Value
        {
            set
            {
                m_Input.text = value.ToString();
                m_Input.onEndEdit.Invoke( m_Input.text );
            }
            get
            {
                return SafeParse( m_Input.text );
            }
        }

        private int SafeParse( string text )
        {
            if( int.TryParse( m_Input.text, out int result ) )
            {
                return result;
            }
            return 0;
        }

        public override void SetValueWithoutNotify( int value )
        {
            m_Input.SetTextWithoutNotify( value.ToString() );
        }

        public override void SetIndeterminateValue()
        {
            m_Input.SetTextWithoutNotify( "---" );
        }

        public override bool Interactable { get => m_Input.interactable; set => m_Input.interactable = value; }
    }
}
