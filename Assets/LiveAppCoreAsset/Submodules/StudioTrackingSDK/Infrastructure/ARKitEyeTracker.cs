using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UniRx;
using System;
using StudioTrackingSDK.Domain;
using UnityEngine.XR.ARKit;
using Unity.Collections;

namespace StudioTrackingSDK.Infrastructure
{
    public class ARKitEyeTracker : MonoBehaviour, IEyeTrackingDomain
    {
        [SerializeField] private ARFaceManager _faceManager;
        private Subject<float> _onEyeBallAngleX = new Subject<float>();
        public IObservable<float> OnEyeBallAngleX => _onEyeBallAngleX;

        private Subject<float> _onEyeBallAngleY = new Subject<float>();
        public IObservable<float> OnEyeBallAngleY => _onEyeBallAngleY;

        private Subject<float> _onEyeBlinkLeft = new Subject<float>();
        public IObservable<float> OnEyeBlinkLeft => _onEyeBlinkLeft;

        private Subject<float> _onEyeBlinkRight = new Subject<float>();
        public IObservable<float> OnEyeBlinkRight => _onEyeBlinkRight;

        public bool IsActive { get; private set; }

        private ARKitFaceSubsystem _faceSubsystem;

        private void OnEnable()
        {
            _faceManager.trackablesChanged.AddListener( OnEyeChanged );
        }

        private void OnDisable()
        {
            _faceManager.trackablesChanged.RemoveListener( OnEyeChanged );
        }

        /// <summary>
        /// 눈동자 정보 변경 이벤트
        /// </summary>
        /// <param name="eventArgs">발생한 이벤트 값</param>
        private void OnEyeChanged(
    ARTrackablesChangedEventArgs<ARFace> eventArgs )
        {
            if( !IsActive )
            {
                return;
            }

            if( ARSession.state != ARSessionState.SessionTracking )
            {
                return;
            }

            foreach( ARFace face in eventArgs.updated )
            {
                if( TryUpdateEyes( face ) )
                {
                    return;
                }
            }

            foreach( ARFace face in eventArgs.added )
            {
                if( TryUpdateEyes( face ) )
                {
                    return;
                }
            }
        }

        private bool TryUpdateEyes( ARFace face )
        {
            if( face == null ||
                face.trackingState != TrackingState.Tracking )
            {
                return false;
            }

            UpdateEyeBlendShape( face );
            UpdateEyeBallDirection( face );

            return true;
        }

        private void UpdateEyeBlendShape( ARFace arFace )
        {
            _faceSubsystem = ( ARKitFaceSubsystem )_faceManager.subsystem;
            using var blendShapesARKit = _faceSubsystem.GetBlendShapeCoefficients( arFace.trackableId, Allocator.Temp );

            for ( var i = 0 ; i < blendShapesARKit.Length ; i++ )
            {
                switch ( blendShapesARKit[ i ].blendShapeLocation )
                {
                    case ARKitBlendShapeLocation.EyeBlinkLeft:
                        var eyeBlinkLeft = 1 - blendShapesARKit[ i ].coefficient;
                        _onEyeBlinkLeft.OnNext( eyeBlinkLeft );
                        break; ;
                    case ARKitBlendShapeLocation.EyeBlinkRight:
                        var eyeBlinkRight = 1 - blendShapesARKit[ i ].coefficient;
                        _onEyeBlinkRight.OnNext( eyeBlinkRight );
                        break;

                    //case ARKitBlendShapeLocation.EyeLookUpLeft:
                    //case ARKitBlendShapeLocation.EyeLookUpRight:
                    //    _avatar.SetEyeLookVertical( -blendShapesARKit[ i ].coefficient );
                    //    break;
                    //case ARKitBlendShapeLocation.EyeLookDownLeft:
                    //case ARKitBlendShapeLocation.EyeLookDownRight:
                    //    _avatar.SetEyeLookVertical( blendShapesARKit[ i ].coefficient );
                    //    break;
                }
            }
        }

        private void UpdateEyeBallDirection( ARFace arFace )
        {
            var eyeLookInLeft = 0f;
            var eyeLookOutLeft = 0f;
            var eyeLookInRight = 0f;
            var eyeLookOutRight = 0f;

            var eyeLookUpLeft = 0f;
            var eyeLookDownLeft = 0f;
            var eyeLookUpRight = 0f;
            var eyeLookDownRight = 0f;

            if ( arFace == null || IsActive == false )
            {
                return;
            }

            // ARKit의 BlendShape 데이터 가져오기
            var blendShapes = _faceSubsystem = ( ARKitFaceSubsystem )_faceManager.subsystem;
            using var blendShapesARKit = _faceSubsystem.GetBlendShapeCoefficients( arFace.trackableId, Allocator.Temp );

            for ( var i = 0 ; i < blendShapesARKit.Length ; i++ )
            {
                switch ( blendShapesARKit[ i ].blendShapeLocation )
                {
                    case ARKitBlendShapeLocation.EyeLookDownLeft:
                        eyeLookDownLeft = blendShapesARKit[ i ].coefficient;
                        break;
                    case ARKitBlendShapeLocation.EyeLookDownRight:
                        eyeLookDownRight = blendShapesARKit[ i ].coefficient;
                        break;
                    case ARKitBlendShapeLocation.EyeLookInLeft:
                        eyeLookInLeft = blendShapesARKit[ i ].coefficient;
                        break;
                    case ARKitBlendShapeLocation.EyeLookInRight:
                        eyeLookInRight = blendShapesARKit[ i ].coefficient;
                        break;
                    case ARKitBlendShapeLocation.EyeLookOutLeft:
                        eyeLookOutLeft = blendShapesARKit[ i ].coefficient;
                        break;
                    case ARKitBlendShapeLocation.EyeLookOutRight:
                        eyeLookOutRight = blendShapesARKit[ i ].coefficient;
                        break;
                    case ARKitBlendShapeLocation.EyeLookUpLeft:
                        eyeLookUpLeft = blendShapesARKit[ i ].coefficient;
                        break;
                    case ARKitBlendShapeLocation.EyeLookUpRight:
                        eyeLookUpRight = blendShapesARKit[ i ].coefficient;
                        break;
                }
            }
            var eyeBallXValue = ( eyeLookOutLeft - eyeLookInLeft + eyeLookInRight - eyeLookOutRight ) / 2.0f;
            var eyeBallYValue = ( eyeLookUpLeft - eyeLookDownLeft + eyeLookUpRight - eyeLookDownRight ) / 2.0f;

            var resultX = Mathf.Clamp(eyeBallXValue, -1f, 1f);
            var resultY = Mathf.Clamp(eyeBallXValue, -1f, 1f);

            _onEyeBallAngleX.OnNext( resultX );
            _onEyeBallAngleY.OnNext( resultY );
        }

        public void SetIsActive( bool isValue )
            => IsActive = isValue;
    }
}