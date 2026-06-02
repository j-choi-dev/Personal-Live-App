using System;
using System.Collections.Generic;
using UniRx;

namespace LiveAppUI.Presenter
{
    public interface IResourceListView : IViewBase
    {
        IObservable<Unit> OnClickClose { get; }
        IObservable<Unit> OnClickCancle { get; }

        void SetTitle( string title );
        void SetList( IReadOnlyList<(string id, string name)> list );
        void ResetList();
    }
}
