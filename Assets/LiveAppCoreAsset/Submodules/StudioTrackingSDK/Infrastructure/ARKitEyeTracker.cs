using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UniRx;
using System;
using StudioTrackingSDK.Domain;
#if !LIVEAPP_ASSETBUNDLE_STANDALONE
using UnityEngine.XR.ARKit;
#endif
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
        [SerializeField]
        private bool _enableTrackingDebugLog = true;

        [SerializeField, Min(1)]
        private int _trackingDebugLogInterval = 30;

        public bool IsActive { get; private set; }
        private bool _resetRequested = false;
        private bool _hasReferenceGaze = false;

        private Vector2 _referenceGaze = Vector2.zero;
        private TrackableId _referenceTrackableId;

#if !LIVEAPP_ASSETBUNDLE_STANDALONE
        private ARKitFaceSubsystem _faceSubsystem;
#endif

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
        private void OnEyeChanged( ARTrackablesChangedEventArgs<ARFace> eventArgs )
        {
#if !LIVEAPP_ASSETBUNDLE_STANDALONE
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
#endif
        }

#if !LIVEAPP_ASSETBUNDLE_STANDALONE
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

            for( var i = 0; i < blendShapesARKit.Length; i++ )
            {
                switch( blendShapesARKit[i].blendShapeLocation )
                {
                    case ARKitBlendShapeLocation.EyeBlinkLeft:
                        var eyeBlinkLeft = 1f - blendShapesARKit[i].coefficient;
                        _onEyeBlinkLeft.OnNext( eyeBlinkLeft );
                        break;

                    case ARKitBlendShapeLocation.EyeBlinkRight:
                        var eyeBlinkRight = 1f - blendShapesARKit[i].coefficient;
                        _onEyeBlinkRight.OnNext( eyeBlinkRight );
                        break;
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

            if( arFace == null || IsActive == false )
            {
                return;
            }

            // ARKit의 BlendShape 데이터 가져오기
            var blendShapes = _faceSubsystem = ( ARKitFaceSubsystem )_faceManager.subsystem;
            using var blendShapesARKit = _faceSubsystem.GetBlendShapeCoefficients( arFace.trackableId, Allocator.Temp );

            for( var i = 0; i < blendShapesARKit.Length; i++ )
            {
                switch( blendShapesARKit[i].blendShapeLocation )
                {
                    case ARKitBlendShapeLocation.EyeLookDownLeft:
                        eyeLookDownLeft = blendShapesARKit[i].coefficient;
                        break;
                    case ARKitBlendShapeLocation.EyeLookDownRight:
                        eyeLookDownRight = blendShapesARKit[i].coefficient;
                        break;
                    case ARKitBlendShapeLocation.EyeLookInLeft:
                        eyeLookInLeft = blendShapesARKit[i].coefficient;
                        break;
                    case ARKitBlendShapeLocation.EyeLookInRight:
                        eyeLookInRight = blendShapesARKit[i].coefficient;
                        break;
                    case ARKitBlendShapeLocation.EyeLookOutLeft:
                        eyeLookOutLeft = blendShapesARKit[i].coefficient;
                        break;
                    case ARKitBlendShapeLocation.EyeLookOutRight:
                        eyeLookOutRight = blendShapesARKit[i].coefficient;
                        break;
                    case ARKitBlendShapeLocation.EyeLookUpLeft:
                        eyeLookUpLeft = blendShapesARKit[i].coefficient;
                        break;
                    case ARKitBlendShapeLocation.EyeLookUpRight:
                        eyeLookUpRight = blendShapesARKit[i].coefficient;
                        break;
                }
            }
            var eyeBallXValue = (eyeLookOutLeft - eyeLookInLeft + eyeLookInRight - eyeLookOutRight) / 2f;
            var eyeBallYValue = (eyeLookUpLeft - eyeLookDownLeft + eyeLookUpRight - eyeLookDownRight) / 2f;

            var currentGaze = new Vector2( eyeBallXValue, eyeBallYValue);

            if (_resetRequested ||
                (_hasReferenceGaze && _referenceTrackableId != arFace.trackableId))
            {
                _referenceGaze = currentGaze;
                _referenceTrackableId = arFace.trackableId;

                _hasReferenceGaze = true;
                _resetRequested = false;
            }

            var outputGaze = _hasReferenceGaze
                ? currentGaze - _referenceGaze
                : currentGaze;

            _onEyeBallAngleX.OnNext( Mathf.Clamp(outputGaze.x, -1f, 1f) );
            _onEyeBallAngleY.OnNext( Mathf.Clamp(outputGaze.y, -1f, 1f) );
        }
#endif

        public void SetIsActive( bool isValue )
            => IsActive = isValue;

        public void ResetTracking()
        {
            _resetRequested = true;
        }
    }
}