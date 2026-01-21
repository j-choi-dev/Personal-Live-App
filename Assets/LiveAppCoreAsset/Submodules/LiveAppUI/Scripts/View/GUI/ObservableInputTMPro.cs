using UnityEngine;
using TMPro;
using UniRx;
using System;

namespace LiveAppUI.View
{
    public class ObservableInputTMPro : ObservableInput
    {
        [SerializeField] private TMP_InputField _input = null;

        private string _prevText;
        private Subject<string> _onValueChanged = new Subject<string>();
        public override IObservable<string> OnEndEdit => Observable.Merge(
            _input.onEndEdit.AsObservable()
                .Do( text =>
                {
                    if( ValidateInput( text ) == false )
                    {
                        _input.SetTextWithoutNotify( _prevText );
                    }
                    else
                    {
                        _prevText = text;
                    }
                } )
                .Where( text => ValidateInput( text ) ),
            _onValueChanged );
        public override string Text 
        {
            set
            {
                if( ValidateInput( value ) )
                {
                    _prevText = value;
                    _input.text = value;
                    _onValueChanged.OnNext( value );
                }
            }
            get => _input.text; 
        }

        public override void SetTextWithoutNotify( string text )
        {
            if( _input.isFocused == false && ValidateInput( text ) )
            {
                _prevText = text ;
                _input.SetTextWithoutNotify( text );
            }
        }

        public override bool Interactable { get => _input.interactable; set => _input.interactable = value; }

        public override void SetIndeterminateValue()
        {
            _input.SetTextWithoutNotify( "---" );
        }

        public override void Select()
        {
            _input.Select();
        }
    }
}
