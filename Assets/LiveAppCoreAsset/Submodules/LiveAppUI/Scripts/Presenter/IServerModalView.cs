using System;
using System.Collections.Generic;
using UniRx;

namespace LiveAppUI.Presenter
{
    public interface IServerModalView
    {
        bool IsActive { get; }
        IObservable<Unit> OnClose { get; }
        IObservable<Unit> OnClicLogin { get; }

        string CurrentID { get; }
        string CurrentPassword { get; }
        int CurrentIndex { get; }

        void SetServerList( IReadOnlyList<string> list );
        void SetActive( bool isActive );
    }
}
