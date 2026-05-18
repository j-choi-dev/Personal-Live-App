using System;
using UniRx;
using UnityEngine;

namespace LiveAppUI.Presenter
{
    public interface ISystemMenuView
    {
        IObservable<Unit> OnClickEmergency { get; }
        IObservable<bool> OnClickLock { get; }
        IObservable<bool> OnClickRec { get; }
    }
}
