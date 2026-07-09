using LiveApp.Util;
using System;
using System.Collections.Generic;
using UniRx;

namespace LiveApp.UI
{
    public interface IListView
    {
        IReadOnlyList<ICellView> Cells { get; }
        IReadOnlyList<string> CurrentSelectedItemList { get; }
        public IReactiveCollection<ICellView> OnCellChanged { get; }
        public IObservable<int> OnSelectedIndex { get; }
        public IObservable<string> OnSelectedId { get; }
        public IObservable<string> OnSelectedDisplayName { get; }

        void AddItem( string id, string displayName );
        void AddItem( string displayName );
        void RemoveItem( string id );
        void Clear();
    }
}
