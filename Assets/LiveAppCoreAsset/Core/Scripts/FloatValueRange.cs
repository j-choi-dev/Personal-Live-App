using System;
using UniRx;
using UnityEngine;

namespace LiveAppCore
{

    public class FloatValueRange : IValueRestriction
    {
        private Subject<Unit> _onChanged = new();
        public IObservable<Unit> OnChanged => _onChanged;

        public float Min { get; private set; }
        public float Max { get; private set; }


        public FloatValueRange( float min, float max )
        {
            this.Min = min;
            this.Max = max;
        }

        public void SetMin( float min )
        {
            this.Min = min;
            _onChanged.OnNext( Unit.Default );
        }
        public void SetMax( float max )
        {
            this.Max = max;
            _onChanged.OnNext( Unit.Default );
        }

        public float Apply( float value )
        {
            return Mathf.Clamp( value, Min, Max );
        }
    }
}
