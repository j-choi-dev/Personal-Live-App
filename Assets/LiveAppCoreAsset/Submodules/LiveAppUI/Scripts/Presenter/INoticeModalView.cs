using System;
using UniRx;

namespace LiveAppUI.Presenter
{
    public interface INoticeModalView : IViewBase
    {
        IObservable<Unit> OnClickYes { get; }

        void SetHeaderTitle(string value);
        void SetMainMessage(string value);
        void SetSubMessage(string value);
    }
}
