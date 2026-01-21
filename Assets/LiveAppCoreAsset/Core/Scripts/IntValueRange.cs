using System;
using UniRx;
using UnityEngine;

namespace LiveAppCore
{
    public class IntValueRange : IValueRestriction
    {
        private Subject<Unit> _onChanged = new();
        public IObservable<Unit> OnChanged => _onChanged;

        public int Min { get; private set; }
        public int Max { get; private set; }


        public IntValueRange( int min, int max )
        {
            this.Min = min;
            this.Max = max;
        }

        public void SetMin( int min )
        {
            this.Min = min;
            _onChanged.OnNext( Unit.Default );
        }
        public void SetMax( int max )
        {
            this.Max = max;
            _onChanged.OnNext( Unit.Default );
        }

        public int Apply( int value )
        {
            return Mathf.Clamp( value, Min, Max );
        }
    }
}
