#if UNITY_EDITOR_OSX

using System;
using StudioTrackingSDK.Domain;
using UniRx;
using UnityEngine;

namespace StudioTrackingSDK.Infrastructure
{
    public sealed class EditorLipSyncVowelSource :
        MonoBehaviour,
        ILipSyncDomain
    {
        [SerializeField, Range(0f, 1f)] private float _a;
        [SerializeField, Range(0f, 1f)] private float _e;
        [SerializeField, Range(0f, 1f)] private float _i;
        [SerializeField, Range(0f, 1f)] private float _o;
        [SerializeField, Range(0f, 1f)] private float _u;

        private readonly Subject<float> _onVowelA = new();
        private readonly Subject<float> _onVowelE = new();
        private readonly Subject<float> _onVowelI = new();
        private readonly Subject<float> _onVowelO = new();
        private readonly Subject<float> _onVowelU = new();

        public IObservable<float> OnVowelA => _onVowelA;
        public IObservable<float> OnVowelE => _onVowelE;
        public IObservable<float> OnVowelI => _onVowelI;
        public IObservable<float> OnVowelO => _onVowelO;
        public IObservable<float> OnVowelU => _onVowelU;

        public float A => _a;
        public float E => _e;
        public float I => _i;
        public float O => _o;
        public float U => _u;

        public bool IsActive { get; private set; }

        private void OnValidate()
        {
            if (!Application.isPlaying || !IsActive)
            {
                return;
            }

            Publish();
        }

        private void Update()
        {
            if (!IsActive)
            {
                return;
            }

            Publish();
        }

        public void SetIsActive(bool value)
        {
            IsActive = value;

            if (value)
            {
                Publish();
            }
        }

        private void Publish()
        {
            _onVowelA.OnNext(_a);
            _onVowelE.OnNext(_e);
            _onVowelI.OnNext(_i);
            _onVowelO.OnNext(_o);
            _onVowelU.OnNext(_u);
        }

        private void OnDestroy()
        {
            DisposeSubject(_onVowelA);
            DisposeSubject(_onVowelE);
            DisposeSubject(_onVowelI);
            DisposeSubject(_onVowelO);
            DisposeSubject(_onVowelU);
        }

        private static void DisposeSubject(Subject<float> subject)
        {
            subject.OnCompleted();
            subject.Dispose();
        }
    }
}

#endif