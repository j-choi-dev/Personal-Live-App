using StudioTrackingSDK.Domain;
using System;
namespace StudioTrackingSDK.Application
{
    public class LipSyncContext : ILipSyncContext
    {
        private ILipSyncDomain _lipSyncDomain;
        public IObservable<float> OnVowelA => _lipSyncDomain.OnVowelA;
        public IObservable<float> OnVowelE => _lipSyncDomain.OnVowelE;
        public IObservable<float> OnVowelI => _lipSyncDomain.OnVowelI;
        public IObservable<float> OnVowelO => _lipSyncDomain.OnVowelO;
        public IObservable<float> OnVowelU => _lipSyncDomain.OnVowelU;

        public float A => _lipSyncDomain.A;
        public float E => _lipSyncDomain.E;
        public float I => _lipSyncDomain.I;
        public float O => _lipSyncDomain.O;
        public float U => _lipSyncDomain.U;

        public bool IsActive => _lipSyncDomain.IsActive;

        public string CurrentSelectedCharacter { get; private set; }

        public LipSyncContext( ILipSyncDomain lipSyncDomain )
        {
            _lipSyncDomain = lipSyncDomain;
        }

        public void SetIsActive( bool isValue )
        {
            _lipSyncDomain.SetIsActive( isValue );
        }

        public void SetCharacterID( string id )
        {
            CurrentSelectedCharacter = id;
        }
    }
}