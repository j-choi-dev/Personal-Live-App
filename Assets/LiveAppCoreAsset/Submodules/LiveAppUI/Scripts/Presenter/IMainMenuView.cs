using System;
using UniRx;

namespace LiveAppUI.Presenter
{
    public interface IMainMenuView : IViewBase
    {
        IObservable<Unit> OnResourceButtonCLick { get; }        
    }
}
