using LiveAppCore;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LiveAppUI.View
{
    public abstract class ObservableInput : MonoBehaviour
    {
        private DateTimeRestriction _dateTimeRestriction;

        public abstract IObservable<string> OnEndEdit { get; }
        public IObservable<string> OnValueChanged => OnEndEdit;
        public abstract string Text { set; get; }
        public string Value { get => Text; set => Text = value; }
        public abstract void SetTextWithoutNotify( string text );
        public void SetValueWithoutNotify( string value ) => SetTextWithoutNotify( value );

        public abstract bool Interactable { get; set; }
        public abstract void SetIndeterminateValue();
        public abstract void Select();

        public void SetRestrictions( List<IValueRestriction> restrictions )
        {
            _dateTimeRestriction = restrictions
                .FirstOrDefault( res => res is DateTimeRestriction )
                as DateTimeRestriction;
        }

        protected bool ValidateInput( string text )
        {
            return _dateTimeRestriction == null
                || _dateTimeRestriction.TryParse( text, out _ );
        }
    }
}
