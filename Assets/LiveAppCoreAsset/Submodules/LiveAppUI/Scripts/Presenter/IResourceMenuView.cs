using System;
using UniRx;

namespace LiveAppUI.Presenter
{
    public interface IResourceMenuView : IViewBase
    {
        IObservable<Unit> OnClickAvatar { get; }
        IObservable<Unit> OnClickStage { get; }
        IObservable<Unit> OnClickProp { get; }
        IObservable<Unit> OnBackButtonClick { get; }
    }
}
