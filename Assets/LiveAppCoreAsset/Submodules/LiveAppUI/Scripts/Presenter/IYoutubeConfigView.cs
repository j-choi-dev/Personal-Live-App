using System;
using UniRx;

namespace LiveAppUI.Presenter
{
    public interface IYoutubeConfigView
    {
        IObservable<Unit> OnPrepareButton { get; }
        IObservable<Unit> OnStartButton { get; }
        IObservable<Unit> OnStopButton { get; }
        string Title { get; }
        string StreamKey { get; }

        void SetIdleStatus();
        void SetPreparing( string message );
        void SetReady( string message );
        void SetStarting( string message );
        void SetLive(string message);
        void SetFailed( string message );
        void GetResolution( out int width, out int height );
    }
}
