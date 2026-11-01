using StudioTrackingSDK.Domain;
using System;
using UniRx;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace StudioTrackingSDK.Infrastructure
{
    public class ARKitFaceTraker : MonoBehaviour, IFaceTrackingDomain
    {
        [SerializeField] private ARFaceManager _faceManager;

        private Subject<float> _onFaceAngleX = new Subject<float>();
        public IObservable<float> OnFaceAngleX => _onFaceAngleX;

        private Subject<float> _onFaceAngleY = new Subject<float>();
        public IObservable<float> OnFaceAngleY => _onFaceAngleY;

        private Subject<float> _onFaceAngleZ = new Subject<float>();
        public IObservable<float> OnFaceAngleZ => _onFaceAngleZ;

        public bool IsActive { get; private set; } = false;
        private bool _resetRequested = false;
        private bool _hasReferenceRotation = false;

        private Quaternion _referenceRotation = Quaternion.identity;
        private TrackableId _referenceTrackableId;

        private void OnEnable()
        {
            _faceManager.trackablesChanged.AddListener( OnFaceChanged );
        }

        private void OnDisable()
        {
            _faceManager.trackablesChanged.RemoveListener( OnFaceChanged );
        }


        /// <summary>
        /// 얼굴 정보 변경 이벤트
        /// </summary>
        /// <param name="eventArgs">발생한 이벤트 값</param>
        private void OnFaceChanged( ARTrackablesChangedEventArgs<ARFace> eventArgs )
        {
            if( IsActive == false )
            {
                return;
            }

            if( ARSession.state != ARSessionState.SessionTracking )
            {
                return;
            }

            // 기존 얼굴의 갱신 데이터를 먼저 처리.
            foreach( ARFace face in eventArgs.updated )
            {
                if( TryUpdateFace( face ) )
                {
                    return;
                }
            }

            // 얼굴이 최초 검출된 프레임도 처리.
            foreach( ARFace face in eventArgs.added )
            {
                if( TryUpdateFace( face ) )
                {
                    return;
                }
            }
        }

        private bool TryUpdateFace( ARFace face )
        {
            if( face == null )
            {
                return false;
            }

            if( face.trackingState != TrackingState.Tracking )
            {
                return false;
            }

            UpdateFaceTransform( face );
            return true;
        }

        /// <summary>
        /// 얼굴 방향을 변경
        /// </summary>
        /// <param name="arFace">ARFace 정보</param>
        private void UpdateFaceTransform( ARFace arFace )
        {
            if( IsActive == false )
            {
                return;
            }

            var currentRotation = arFace.transform.rotation;

            if (_resetRequested ||
                (_hasReferenceRotation && _referenceTrackableId != arFace.trackableId))
            {
                _referenceRotation = currentRotation;
                _referenceTrackableId = arFace.trackableId;

                _hasReferenceRotation = true;
                _resetRequested = false;
            }

            var outputRotation = _hasReferenceRotation
                ? Quaternion.Inverse(_referenceRotation) * currentRotation
                : currentRotation;

            var euler = outputRotation.eulerAngles;

            _onFaceAngleX.OnNext(Mathf.DeltaAngle(0f, euler.x) * 2f);
            _onFaceAngleY.OnNext(Mathf.DeltaAngle(0f, euler.y));
            _onFaceAngleZ.OnNext(Mathf.DeltaAngle(0f, euler.z) * 2f);
        }

        public void SetIsActive( bool isValue )
            => IsActive = isValue;

        public void ResetTracking()
        {
            _resetRequested = true;
        }
    }
}
