using System;
using UnityEngine;

namespace StudioTrackingSDK.Domain
{
    public interface ILipSyncDomain
    {
        IObservable<float> OnVowelA { get; }
        IObservable<float> OnVowelE { get; }
        IObservable<float> OnVowelI { get; }
        IObservable<float> OnVowelO { get; }
        IObservable<float> OnVowelU { get; }

        // 현재 스무딩된 값
        public float A { get; }
        public float E { get; }
        public float I { get; }
        public float O { get; }
        public float U { get; }
        bool IsActive { get; }
        void SetIsActive( bool isValue );
    }
}
