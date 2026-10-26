using StudioTrackingSDK.Domain;
using System;
using UniRx;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace StudioTrackingSDK.Infrastructure
{
    public class ARKitBodyTraker : MonoBehaviour, IBodyTrackingDomain
    {
        [SerializeField] private ARFaceManager _faceManager;

        private Subject<float> _onBodyAngleX = new Subject<float>();
        public IObservable<float> OnBodyAngleXChanged => _onBodyAngleX;

        private Subject<float> _onBodyAngleY = new Subject<float>();
        public IObservable<float> OnBodyAngleYChanged => _onBodyAngleY;

        private Subject<float> _onBodyAngleZ = new Subject<float>();
        public IObservable<float> OnBodyAngleZChanged => _onBodyAngleZ;

        public bool IsActive { get; private set; } = false;

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
            if (!IsActive)
            {
                Debug.Log("OnFaceChanged ignored: Tracker inactive.");
                return;
            }

            if (ARSession.state != ARSessionState.SessionTracking)
            {
                Debug.Log($"OnFaceChanged ignored: ARSession={ARSession.state}");
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
            if (IsActive == false)
            {
                Debug.Log("UpdateBodyTransform Break");
                return;
            }

            Debug.Log("UpdateBodyTransform ...;");

            var bodyRotation = arFace.transform.rotation;
            var x = NormalizeAngle(bodyRotation.eulerAngles.x) * 2f;
            var y = NormalizeAngle(bodyRotation.eulerAngles.y);
            var z = NormalizeAngle(bodyRotation.eulerAngles.z) * 2f;

            _onBodyAngleX.OnNext(x);
            _onBodyAngleY.OnNext(y);
            _onBodyAngleZ.OnNext(z);
        }

        private float NormalizeAngle(float angle)
        {
            if (angle > 180)
            {
                return angle - 360;
            }
            return angle;
        }

        public void SetIsActive(bool isValue)
            => IsActive = isValue;
    }
}
