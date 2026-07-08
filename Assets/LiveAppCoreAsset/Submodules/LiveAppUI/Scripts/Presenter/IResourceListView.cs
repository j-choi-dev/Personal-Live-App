using System;
using System.Collections.Generic;
using UniRx;

namespace LiveAppUI.Presenter
{
    public interface IResourceListView : IViewBase
    {
        IObservable<int> OnServerChange { get; }
        IObservable<Unit> OnClickClose { get; }
        IObservable<Unit> OnClickCancle { get; }
        
        int CurrentServerIndex { get; }

        void SetTitle( string title );
        void SetServerList( IReadOnlyList<string> servers );
        void SetServerItem( int index );
        void SetList( IReadOnlyList<(string id, string name)> list );
        void AddListItem( string id, string name );
        void RemoveListItem( string id );
        void ResetList();
    }
}
