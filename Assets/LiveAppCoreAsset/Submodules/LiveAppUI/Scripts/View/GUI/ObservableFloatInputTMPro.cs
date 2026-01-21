using UnityEngine;
using TMPro;
using System;
using UniRx;

namespace LiveAppUI.View
{
    public class ObservableFloatInputTMPro : ObservableFloatInput
    {
        [SerializeField] private TMP_InputField _input = null;
        [SerializeField] private int _numDigits = 4;

        public override bool Interactable { get => _input.interactable; set => _input.interactable = value; }

        public override IObservable<float> OnValueChanged => _input.onEndEdit.AsObservable()
            .Select( _ => ApplyRange( SafeParse() ) );

        public override float Value
        {
            set
            {
                _input.text = ApplyRange( value ).ToString( $"F{_numDigits}" );
                _input.onEndEdit.Invoke( _input.text );
            }
            get
            {
                return SafeParse();
            }
        }

        private float SafeParse()
        {
            if( float.TryParse( _input.text, out float result ) )
            {
                return result;
            }
            return 0;
        }

        public override void SetValueWithoutNotify( float value )
        {
            if( _input.isFocused == false )
            {
                _input.SetTextWithoutNotify( ApplyRange( value ).ToString() );
            }
        }

        public override void SetIndeterminateValue()
        {
            _input.SetTextWithoutNotify( "---" );
        }
    }
}
