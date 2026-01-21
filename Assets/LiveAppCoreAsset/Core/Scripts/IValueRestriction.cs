using System;
using System.Collections.Generic;
using UniRx;

namespace LiveAppCore
{
    public interface IValueRestriction
    {
        IObservable<Unit> OnChanged { get; }
    }

    public interface IValueOptions : IValueRestriction
    {
        IReadOnlyList<string> GetOptions();
    }
}
