using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Linq;
using LiveAppCore.Linq;
using UniRx;

namespace LiveAppUI.View
{
    public class ObservableDropdownWithInput : ObservableDropdown
    {
        const string INDETERMINATE_VALUE = "---";
        [SerializeField] private TMP_Dropdown _dropdown = null;
        [SerializeField] private TMP_InputField _inputField = null;
        [SerializeField] private TMP_Text _placeHolder = null;
        [SerializeField] private float _optionWidthOffset = 50;

        public override int Value
        {
            get => _dropdown.value;
            set
            {
                _dropdown.value = value;
                _inputField.SetTextWithoutNotify( _dropdown.options[value].text );
            }
        }

        private Subject<int> _onValueChanged = new Subject<int>();
        public override IObservable<int> OnValueChanged => _onValueChanged;

        public override bool Interactable
        {
            get => _dropdown.interactable;
            set
            {
                _dropdown.interactable = value;
                _inputField.interactable = value;
            }
        }

        public override string Text
        {
            get => _dropdown.captionText.text;
            set
            {
                int index = _dropdown.options.IndexOf( opt => opt.text == value );
                if( index < 0 )
                {
                    throw new Exception( $"Invalid Value: {value} is not contains {_dropdown}" );
                }
                _inputField.SetTextWithoutNotify( value );
                _dropdown.value = index;
            }
        }

        public override IObservable<string> OnTextChanged => OnValueChanged.Select( index => _dropdown.options[index].text );

        public override List<string> Options
        {
            get
            {
                return _dropdown.options
                    .Select( option => option.text )
                    .ToList();
            }
        }

        private void Awake()
        {
            Debug.Assert( _dropdown.placeholder != null, this );
            Debug.Assert( _dropdown.placeholder == _placeHolder, this );

            _placeHolder.text = INDETERMINATE_VALUE;

            _inputField.onEndEdit.AsObservable()
                .Subscribe( text =>
                {
                    int index = _dropdown.options.IndexOf( opt => opt.text.ToLower().Contains(text.ToLower()) );
                    if( index < 0 )
                    {
                        _inputField.SetTextWithoutNotify( _dropdown.captionText.text );
                        return;
                    }
                    _dropdown.SetValueWithoutNotify( index );
                    _onValueChanged.OnNext( index );
                } )
                .AddTo( this );


            _dropdown.template.anchorMin = new Vector2( 1, 0 );
            _dropdown.template.anchorMax = new Vector2( 1, 0 );
        }

        private void OnEnable()
        {
            UpdateTemplateWidth();
        }

        public override void SetOptions( IReadOnlyList<string> options )
        {
            if( _dropdown.value >= _dropdown.options.Count )
            {
                _dropdown.options = options
                    .Select( str => new TMP_Dropdown.OptionData( str ) )
                    .ToList();
            }
            else
            {
                var newIndex = options.IndexOf( opt => opt == Text );
                _dropdown.options = options
                    .Select( str => new TMP_Dropdown.OptionData( str ) )
                    .ToList();
                SetValueWithoutNotify( newIndex );
            }

            UpdateTemplateWidth();
        }

        public override void SetValueWithoutNotify( int value )
        {
            if( value != _dropdown.value )
            {
                _dropdown.SetValueWithoutNotify( value );
            }
            else
            {
                _dropdown.RefreshShownValue();
            }

            if( 0 <= value && value < _dropdown.options.Count  )
            {
                _inputField.SetTextWithoutNotify( _dropdown.options[value].text );
            }
            else
            {
                _inputField.SetTextWithoutNotify( "" );
            }
        }

        public override void SetTextWithoutNotify( string text )
        {
            int index = _dropdown.options.IndexOf(opt => opt.text == text);
            if( index < 0 )
            {
                _dropdown.captionText.text = text;
            }
            else
            {
                SetValueWithoutNotify( index );
            }
        }

        private void UpdateTemplateWidth()
        {
            float widthDelta = (_dropdown.transform as RectTransform).rect.width;
            foreach( var opt in _dropdown.options )
            {
                var pref = _dropdown.itemText.GetPreferredValues( opt.text );
                widthDelta = Mathf.Max( pref.x + _optionWidthOffset, widthDelta );
            }

            var sizeDelta = _dropdown.template.sizeDelta;
            sizeDelta.x = widthDelta;
            _dropdown.template.sizeDelta = sizeDelta;
        }

        public override void SetIndeterminateValue()
        {
            _dropdown.SetValueWithoutNotify( -1 );
            _inputField.text = INDETERMINATE_VALUE;
        }
    }
}
