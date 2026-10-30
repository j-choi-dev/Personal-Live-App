using StudioTrackingSDK.Domain;
using System;
using UniRx;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace StudioTrackingSDK.Infrastructure
{
    public class ARKitBodyTracker : MonoBehaviour, IBodyTrackingDomain
    {
        [SerializeField] private ARFaceManager _faceManager;
        [SerializeField, Min(0f)] private float _bodyRollGain = 1f;
        [SerializeField, Min(0f)] private float _maximumBodyRoll = 10f;
        [SerializeField, Min(0f)] private float _rollDeadZone = 0.5f;

        [SerializeField]
        private bool _invertBodyRoll;

        private Subject<float> _onBodyAngleX = new Subject<float>();
        public IObservable<float> OnBodyAngleXChanged => _onBodyAngleX;

        private Subject<float> _onBodyAngleY = new Subject<float>();
        public IObservable<float> OnBodyAngleYChanged => _onBodyAngleY;

        private Subject<float> _onBodyAngleZ = new Subject<float>();
        public IObservable<float> OnBodyAngleZChanged => _onBodyAngleZ;

        public bool IsActive { get; private set; } = false;
        private bool _resetRequested = false;
        private bool _hasReferenceRotation = false;

        private Quaternion _referenceRotation = Quaternion.identity;
        private TrackableId _referenceTrackableId;

        private void OnEnable()
        {
            _faceManager.trackablesChanged.AddListener(OnBodyChanged);
        }

        private void OnDisable()
        {
            _faceManager.trackablesChanged.RemoveListener(OnBodyChanged);
        }


        /// <summary>
        /// 얼굴 정보 변경 이벤트
        /// </summary>
        /// <param name="eventArgs">발생한 이벤트 값</param>
        private void OnBodyChanged(ARTrackablesChangedEventArgs<ARFace> eventArgs)
        {
            if ( IsActive == false )
            {
                return;
            }

            if (ARSession.state != ARSessionState.SessionTracking)
            {
                return;
            }

            // 기존 얼굴의 갱신 데이터를 먼저 처리.
            foreach (ARFace face in eventArgs.updated)
            {
                if (TryUpdateBody(face))
                {
                    return;
                }
            }

            // 얼굴이 최초 검출된 프레임도 처리.
            foreach (ARFace face in eventArgs.added)
            {
                if (TryUpdateBody(face))
                {
                    return;
                }
            }

            Debug.Log("OnBodyChanged ignored: No trackable ARFace found.");
        }

        private bool TryUpdateBody(ARFace face)
        {
            if (face == null)
            {
                return false;
            }

            if (face.trackingState != TrackingState.Tracking)
            {
                Debug.Log($"Face tracking unavailable: {face.trackingState}");
                return false;
            }

            UpdateBodyTransform(face);
            return true;
        }

        /// <summary>
        /// 몸 회전값을 변경
        /// </summary>
        /// <param name="arFace">ARFace 정보</param>
        private void UpdateBodyTransform(ARFace arFace)
        {
            if (IsActive == false || arFace == null)
            {
                return;
            }

            var currentRotation = arFace.transform.rotation;

            if (_resetRequested ||
                (_hasReferenceRotation == false || _referenceTrackableId != arFace.trackableId))
            {
                _referenceRotation = currentRotation;
                _referenceTrackableId = arFace.trackableId;

                _hasReferenceRotation = true;
                _resetRequested = false;
            }

            var relativeRotation = Quaternion.Inverse(_referenceRotation) * currentRotation;
            var roll = ExtractZTwistDegrees(relativeRotation);
            var amount = Mathf.Max( 0f, Mathf.Abs(roll) - _rollDeadZone );
            var outputZ = Mathf.Sign(roll) * amount * _bodyRollGain;

            if (_invertBodyRoll)
            {
                outputZ = -outputZ;
            }

            outputZ = Mathf.Clamp( outputZ, -_maximumBodyRoll, _maximumBodyRoll);

            _onBodyAngleX.OnNext(0f);
            _onBodyAngleY.OnNext(0f);
            _onBodyAngleZ.OnNext(outputZ);
        }

        private static float ExtractZTwistDegrees(Quaternion rotation)
        {
            var length = Mathf.Sqrt( rotation.z * rotation.z + rotation.w * rotation.w);
            if (length < 0.00001f)
            {
                return 0f;
            }
            var z = rotation.z / length;
            var w = rotation.w / length;

            var angle = 2f * Mathf.Atan2(z, w) * Mathf.Rad2Deg;

            return Mathf.DeltaAngle(0f, angle);
        }

        public void SetIsActive(bool isValue)
            => IsActive = isValue;

        public void ResetTracking()
        {
            _resetRequested = true;
        }
    }
}
