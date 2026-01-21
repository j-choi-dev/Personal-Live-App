using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UniRx;
using System;
using StudioTrackingSDK.Domain;

namespace StudioTrackingSDK.Infrastructure
{
    public class ARKitFaceTrakingController : MonoBehaviour, IFaceTrackingDomain
    {
        [SerializeField] private ARFaceManager _faceManager;
        private Subject<float> _onFaceAngleX = new Subject<float>();
        private Subject<float> _onFaceAngleY = new Subject<float>();
        private Subject<float> _onFaceAngleZ = new Subject<float>();

        public IObservable<float> OnFaceAngleX => _onFaceAngleX;

        public IObservable<float> OnFaceAngleY => _onFaceAngleY;

        public IObservable<float> OnFaceAngleZ => _onFaceAngleZ;

        public bool IsActive { get; private set; } = false;

        private void OnEnable()
        {
            _faceManager.facesChanged += OnFaceChanged;
        }

        private void OnDisable()
        {
            _faceManager.facesChanged -= OnFaceChanged;
        }


        /// <summary>
        /// 얼굴 정보 변경 이벤트
        /// </summary>
        /// <param name="eventArgs">발생한 이벤트 값</param>
        private void OnFaceChanged( ARFacesChangedEventArgs eventArgs )
        {
            if ( eventArgs.updated.Count != 0 )
            {
                var arFace = eventArgs.updated[ 0 ];
                if ( arFace.trackingState == TrackingState.Tracking
                    && ( ARSession.state > ARSessionState.Ready ) )
                {
                    UpdateFaceTransform( arFace );
                }
            }
        }

        /// <summary>
        /// 얼굴 방향dmf 변경
        /// </summary>
        /// <param name="arFace">ARFace 정보</param>
        private void UpdateFaceTransform( ARFace arFace )
        {
            // 얼굴의 회전 정보 취득
            var faceRotation = arFace.transform.rotation;

            var x = NormalizeAngle( faceRotation.eulerAngles.x ) * 2f;
            var y = NormalizeAngle( faceRotation.eulerAngles.y );
            var z = NormalizeAngle( faceRotation.eulerAngles.z ) * 2f;

            _onFaceAngleX.OnNext( x );
            _onFaceAngleY.OnNext( y );
            _onFaceAngleZ.OnNext( z );
        }

        private float NormalizeAngle( float angle )
        {
            if ( angle > 180 )
            {
                return angle - 360;
            }
            return angle;
        }

        public void SetIsActive( bool isValue )
            => IsActive = isValue;
    }
}
