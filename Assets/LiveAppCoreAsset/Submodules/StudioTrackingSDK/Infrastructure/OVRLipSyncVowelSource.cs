using StudioTrackingSDK.Domain;
using System;
using UniRx;
using UnityEngine;

namespace StudioTrackingSDK.Infrastructure
{
    public class OVRLipSyncVowelSource : MonoBehaviour, ILipSyncDomain
    {
        [SerializeField] private OVRLipSyncContext _lipSyncContext;
        [SerializeField, Range(0f, 0.2f)] private float _noiseGate = 0.03f; 
        // Viseme 값이 입력을 따라가는 속도
        [SerializeField, Min(0.1f)] private float _responseSpeed = 25f;

        // 이 값 이상 달라졌을 때만 OnNext를 호출
        [SerializeField, Min(0f)] private float _publishEpsilon = 0.001f;

        // Subject는 이 클래스만 값을 발행할 수 있도록 private으로 둔다.
        private Subject<float> _onVowelA = new Subject<float>();
        private Subject<float> _onVowelE = new Subject<float>();
        private Subject<float> _onVowelI = new Subject<float>();
        private Subject<float> _onVowelO = new Subject<float>();
        private Subject<float> _onVowelU = new Subject<float>();

        // 외부에는 읽기 전용 IObservable만 공개한다.
        public IObservable<float> OnVowelA => _onVowelA;
        public IObservable<float> OnVowelE => _onVowelE;
        public IObservable<float> OnVowelI => _onVowelI;
        public IObservable<float> OnVowelO => _onVowelO;
        public IObservable<float> OnVowelU => _onVowelU;

        // 현재 스무딩된 값
        public float A { get; private set; }
        public float E { get; private set; }
        public float I { get; private set; }
        public float O { get; private set; }
        public float U { get; private set; }

        public bool IsActive { get; private set; }

        private float _lastPublishedA = float.NaN;
        private float _lastPublishedE = float.NaN;
        private float _lastPublishedI = float.NaN;
        private float _lastPublishedO = float.NaN;
        private float _lastPublishedU = float.NaN;

        private void Reset()
        {
            _lipSyncContext = GetComponent<OVRLipSyncContext>();
        }

        private void Awake()
        {
            if( _lipSyncContext == null )
            {
                _lipSyncContext = GetComponent<OVRLipSyncContext>();
            }
        }

        private void Update()
        {
            if( _lipSyncContext == null )
            {
                return;
            }

            OVRLipSync.Frame frame = _lipSyncContext.Frame;

            if( frame == null || frame.Visemes == null )
            {
                return;
            }

            int requiredLength = (int)OVRLipSync.Viseme.ou + 1;

            if( frame.Visemes.Length < requiredLength )
            {
                return;
            }
            if( IsActive == false )
            {
                return;
            }

            float targetA = ReadViseme(frame, OVRLipSync.Viseme.aa);
            float targetE = ReadViseme(frame, OVRLipSync.Viseme.E);
            float targetI = ReadViseme(frame, OVRLipSync.Viseme.ih);
            float targetO = ReadViseme(frame, OVRLipSync.Viseme.oh);
            float targetU = ReadViseme(frame, OVRLipSync.Viseme.ou);

            // 프레임레이트에 독립적인 지수 스무딩
            float lerpFactor =
            1f - Mathf.Exp(-_responseSpeed * Time.deltaTime);

            A = Mathf.Lerp( A, targetA, lerpFactor );
            E = Mathf.Lerp( E, targetE, lerpFactor );
            I = Mathf.Lerp( I, targetI, lerpFactor );
            O = Mathf.Lerp( O, targetO, lerpFactor );
            U = Mathf.Lerp( U, targetU, lerpFactor );

            PublishIfChanged( _onVowelA, A, ref _lastPublishedA );
            PublishIfChanged( _onVowelE, E, ref _lastPublishedE );
            PublishIfChanged( _onVowelI, I, ref _lastPublishedI );
            PublishIfChanged( _onVowelO, O, ref _lastPublishedO );
            PublishIfChanged( _onVowelU, U, ref _lastPublishedU );
        }

        private float ReadViseme( OVRLipSync.Frame frame, OVRLipSync.Viseme viseme )
        {
            float value = Mathf.Clamp01(
            frame.Visemes[(int)viseme]);

            // noiseGate 이하는 0으로 만들고,
            // noiseGate~1 구간을 다시 0~1로 정규화한다.
            return Mathf.InverseLerp( _noiseGate, 1f, value );
        }

        private void PublishIfChanged( Subject<float> subject, float value, ref float lastPublishedValue )
        {
            if( !float.IsNaN( lastPublishedValue ) &&
                Mathf.Abs( value - lastPublishedValue ) < _publishEpsilon )
            {
                return;
            }

            lastPublishedValue = value;
            subject.OnNext( value );
        }

        private void OnDestroy()
        {
            CompleteAndDispose( _onVowelA );
            CompleteAndDispose( _onVowelE );
            CompleteAndDispose( _onVowelI );
            CompleteAndDispose( _onVowelO );
            CompleteAndDispose( _onVowelU );
        }

        private static void CompleteAndDispose( Subject<float> subject )
        {
            subject.OnCompleted();
            subject.Dispose();
        }

        public void SetIsActive( bool isValue ) 
            => IsActive = isValue;
    }
}
