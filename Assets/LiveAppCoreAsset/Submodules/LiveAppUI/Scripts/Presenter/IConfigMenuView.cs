using System;

namespace LiveAppUI.Presenter
{
    public interface IConfigMenuView : IViewBase
    {
        IObservable<bool> OnConnectionChanged { get; }
        IObservable<bool> OnStreamingChanged { get; }
        IObservable<bool> OnRecordingChanged { get; }

        string EndPoint { get; }
        string AgentToken { get; }

        void SetEndPointWithoutNotify( string val );
        void SetAgentTokenWithoutNotify( string val );
        void SetLogText( string text );
        void AddLogText( string text );
    }
}
