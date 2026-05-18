using System;
using UniRx;


namespace LiveAppUI.Presenter
{
    public interface IMainMenuView
    {
        IObservable<Unit> OnClickEmergency { get; }
        IObservable<bool> OnRecordingChanged { get; }
    }
}
