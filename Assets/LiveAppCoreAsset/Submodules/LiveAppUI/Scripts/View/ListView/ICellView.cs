using System;
using UniRx;

namespace LiveApp.UI
{
    public interface ICellView
    {
        IObservable<Unit> OnSelected { get; }
        bool IsUsable { get; }
        string ID { get; }
        void SetIsUsable( bool isVal );
        void SetItem( string text, string id );
        void SetIdIfNull( string id );
    }
}
