using System;
using System.Collections.Generic;
using UniRx;

namespace LiveApp.UI
{
    public interface IListView
    {
        IReadOnlyList<ICellView> Cells { get; }
        IObservable<int> OnSelectedIndex { get; }
        IObservable<string> OnSelectedId { get; }
        IObservable<string> OnSelectedDisplayName { get; }

        void AddItem( string id, string displayName );

        void AddItem( string displayName );
    }
}
