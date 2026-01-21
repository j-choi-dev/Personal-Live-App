using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Linq;
using UniRx;
using LiveAppCore.Linq;

namespace LiveAppUI.View
{
    public class ObservableDropdownTMPro : ObservableDropdown
    {
        const string INDETERMINATE_VALUE = "---";

        [SerializeField] private TMP_Dropdown _dropdown = null;
        [SerializeField] private TMP_Text m_PlaceHolder = null;
        [SerializeField] private float _optionWidthOffset = 50;

        public override int Value { get => _dropdown.value; set => _dropdown.value = value; }
        public override IObservable<int> OnValueChanged => _dropdown.onValueChanged.AsObservable();

        public override bool Interactable { get => _dropdown.interactable; set => _dropdown.interactable = value; }

        public override string Text
        {
            get
            {
                if( _dropdown.value < 0 )
                {
                    return INDETERMINATE_VALUE;
                }

                var idx = _dropdown.value;
                return _dropdown.options[idx].text;
            }
            set
            {
                int index = _dropdown.options.IndexOf( opt => opt.text == value );
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
            Debug.Assert( _dropdown.placeholder == m_PlaceHolder, this );

            m_PlaceHolder.text = INDETERMINATE_VALUE;
            OnValueChanged.Select( index => _dropdown.options[ index ].text );

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
        }

        public override void SetTextWithoutNotify( string text )
        {
            int index = _dropdown.options.IndexOf(opt => opt.text == text);
            SetValueWithoutNotify( index );
        }

        private void UpdateTemplateWidth()
        {
            float widthDelta = (_dropdown.transform as RectTransform).rect.width;
            string maxLenText = string.Empty;
            for( int i = 0; i < _dropdown.options.Count; ++i )
            {
                var opt = _dropdown.options[i];
                if( maxLenText.Length < opt.text.Length )
                {
                    maxLenText = opt.text;
                }
            }
            var pref = _dropdown.itemText.GetPreferredValues( maxLenText );
            widthDelta = Mathf.Max( pref.x + _optionWidthOffset, widthDelta );

            var sizeDelta = _dropdown.template.sizeDelta;
            sizeDelta.x = widthDelta;
            _dropdown.template.sizeDelta = sizeDelta;
        }

        public override void SetIndeterminateValue()
        {
            _dropdown.SetValueWithoutNotify( -1 );
        }
    }
}
