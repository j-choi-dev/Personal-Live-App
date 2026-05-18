using System;
using System.Collections.Generic;
using UniRx;

namespace LiveAppUI.Presenter
{
    public interface IRoomModalView
    {
        bool IsActive { get; }
        IObservable<Unit> OnClose { get; }
        IObservable<Unit> OnClickExit { get; }
        IObservable<Unit> OnClickEnter { get; }

        string CurrentName { get; }
        int CurrenIndex { get; }

        void SetRoomList( IReadOnlyList<string> list );
        void SetActive( bool isActive );
    }
}
