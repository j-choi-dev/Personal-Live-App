using System;
using System.Collections.Generic;
using UniRx;

namespace LiveApp.UI
{
    /// <summary>
    /// IReadOnlyReactiveCollection 형태의 하나의 list를 사용한 listview
    /// </summary>
    public interface IListViewWithOneList
    {
        public IReadOnlyReactiveCollection<ICellView> OnCellChanged { get; }
        public IObservable<int> OnSelectedIndex { get; }
        public IObservable<string> OnSelectedId { get; }
        public IObservable<string> OnSelectedDiplayName { get; }
        public IReadOnlyList<string> CurrentSelectedItemList { get; }

        void AddItem(string id, string displayname);
        void AddItem(string displayname);
        void RemoveItem(string id);
        void Clear();


    }
}
