using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UniRx;
using System;
using StudioTrackingSDK.Domain;

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
            if( !IsActive )
            {
                Debug.Log( "OnFaceChanged ignored: Tracker inactive." );
                return;
            }

            if( ARSession.state != ARSessionState.SessionTracking )
            {
                Debug.Log( $"OnFaceChanged ignored: ARSession={ARSession.state}" );
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

            Debug.Log( "OnFaceChanged ignored: No trackable ARFace found." );
        }

        private bool TryUpdateFace( ARFace face )
        {
            if( face == null )
            {
                return false;
            }

            if( face.trackingState != TrackingState.Tracking )
            {
                Debug.Log( $"Face tracking unavailable: {face.trackingState}" );
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
                Debug.Log( "UpdateFaceTransform Break" );
                return;
            }

            Debug.Log( "UpdateFaceTransform ...;" );

            var faceRotation = arFace.transform.rotation;
            var x = NormalizeAngle(faceRotation.eulerAngles.x) * 2f;
            var y = NormalizeAngle(faceRotation.eulerAngles.y);
            var z = NormalizeAngle(faceRotation.eulerAngles.z) * 2f;

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
