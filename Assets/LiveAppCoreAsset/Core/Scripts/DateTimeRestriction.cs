using System;
using UniRx;

namespace LiveAppCore
{
    public class DateTimeRestriction : IValueRestriction
    {
        public IObservable<Unit> OnChanged => Observable.Never<Unit>();

        public bool TryParse( string value, out DateTime dateTime )
        {
            return DateTime.TryParse( value, out dateTime );
        }
    }
}
