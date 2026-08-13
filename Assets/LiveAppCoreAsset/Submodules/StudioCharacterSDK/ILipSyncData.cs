using System;

namespace StudioCharacterSDK.Domain
{
    public interface ILipSyncData
    {
        IObservable<float> OnChangeMouthForm { get; }
        IObservable<float> OnChangeMouthOpen { get; }
        IObservable<LipSyncVowelData> OnChangeLipSync { get; }

        void SetLipSync( LipSyncVowelData value );
        void SetMouthForm( float value );
        void SetMouthOpen( float value );
    }
}
