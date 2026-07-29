using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARKit;
using Unity.Collections;
using TMPro;
using StudioCharacterSDK.Infrastructure;

namespace StudioTrackingSDK.Infrastructure
{
    public class FaceTracking : MonoBehaviour
    {
        [SerializeField] private ARFaceManager faceManager; // From FaceControl
        [SerializeField] private StudioAvatar _avatar; // From Resource
        [SerializeField] private TMP_Text _logHeader;
        [SerializeField] private TMP_Text _logDetail;
        [SerializeField] private TMP_Text _logResult;

        private ARKitFaceSubsystem _faceSubsystem;

        private void Awake()
        {
            UnityEngine.Application.targetFrameRate = 60; // TODO 제거 대상? @Choi 26.07.08
        }

        private void OnEnable()
        {
            faceManager.trackablesChanged.AddListener( OnFaceChanged );
        }

        private void OnDisable()
        {
            faceManager.trackablesChanged.RemoveListener( OnFaceChanged );
        }

        /// <summary>
        /// 얼굴 정보 변경 이벤트
        /// </summary>
        /// <param name="eventArgs">발생한 이벤트 값</param>
        private void OnFaceChanged( ARTrackablesChangedEventArgs<ARFace> eventArgs )
        {
            if( eventArgs.updated.Count != 0 )
            {
                var arFace = eventArgs.updated[0];
                if( arFace.trackingState == TrackingState.Tracking
                    && ( ARSession.state > ARSessionState.Ready ) )
                {
                    UpdateFaceTransform( arFace );
                    UpdateEyeBlendShape( arFace );
                    UpdateEyeBallDirection( arFace );
                    UpdateMouthBlendShape( arFace );
                    UpdateBodyBlendShape( arFace );
                }
            }
        }

        /// <summary>
        /// 얼굴 방향dmf 변경
        /// </summary>
        /// <param name="arFace">ARFace 정보</param>
        private void UpdateFaceTransform( ARFace arFace )
        {
            // 얼굴의 위치 정보 취득
            var faceRotation = arFace.transform.rotation;

            var x = NormalizeAngle( faceRotation.eulerAngles.x ) * 2f;
            var y = NormalizeAngle( faceRotation.eulerAngles.y );
            var z = NormalizeAngle( faceRotation.eulerAngles.z ) * 2f;

            // 새로운 얼굴 회전값을 변수에 대입
            _avatar.SetFaceAngleX( y );
            _avatar.SetFaceAngleY( x );
            _avatar.SetFaceAngleZ( z );
        }

        /// <summary>
        /// 표정 정보를 변경
        /// </summary>
        /// <param name="arFace">ARFace 정보</param>
        private void UpdateEyeBlendShape( ARFace arFace )
        {
            _faceSubsystem = ( ARKitFaceSubsystem )faceManager.subsystem;
            using var blendShapesARKit = _faceSubsystem.GetBlendShapeCoefficients( arFace.trackableId, Allocator.Temp );

            for( var i = 0; i<blendShapesARKit.Length; i++ )
            {
                switch( blendShapesARKit[i].blendShapeLocation )
                {
                    case ARKitBlendShapeLocation.EyeBlinkLeft:
                        _avatar.SetEyeBlinkLeft( 1 - blendShapesARKit[i].coefficient );
                        break; ;
                    case ARKitBlendShapeLocation.EyeBlinkRight:
                        _avatar.SetEyeBlinkRight( 1 - blendShapesARKit[i].coefficient );
                        break;

                    case ARKitBlendShapeLocation.EyeLookUpLeft:
                    case ARKitBlendShapeLocation.EyeLookUpRight:
                        _avatar.SetEyeBallAngleY( -blendShapesARKit[i].coefficient );
                        break;
                    case ARKitBlendShapeLocation.EyeLookDownLeft:
                    case ARKitBlendShapeLocation.EyeLookDownRight:
                        _avatar.SetEyeBallAngleX( blendShapesARKit[i].coefficient );
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

            if( arFace == null || _avatar == null ) return;

            // ARKit의 BlendShape 데이터 가져오기
            var blendShapes = _faceSubsystem = ( ARKitFaceSubsystem )faceManager.subsystem;
            using var blendShapesARKit = _faceSubsystem.GetBlendShapeCoefficients( arFace.trackableId, Allocator.Temp );

            for( var i = 0; i<blendShapesARKit.Length; i++ )
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
            var eyeBallXValue = ( eyeLookOutLeft - eyeLookInLeft + eyeLookInRight - eyeLookOutRight ) / 2.0f;
            var eyeBallYValue = ( eyeLookUpLeft - eyeLookDownLeft + eyeLookUpRight - eyeLookDownRight ) / 2.0f;

            var resultX = Mathf.Clamp( eyeBallXValue, -1f, 1f );
            var resultY = Mathf.Clamp( eyeBallXValue, -1f, 1f );

            _avatar.SetEyeBallAngleX( resultX );
            _avatar.SetEyeBallAngleY( resultY );
        }

        /// <summary>
        /// 표정 정보를 변경
        /// </summary>
        /// <param name="arFace">ARFace 정보</param>
        private void UpdateMouthBlendShape( ARFace arFace )
        {
            _faceSubsystem = ( ARKitFaceSubsystem )faceManager.subsystem;
            using var blendShapesARKit = _faceSubsystem.GetBlendShapeCoefficients( arFace.trackableId, Allocator.Temp );
            for( var i = 0; i<blendShapesARKit.Length; i++ )
            {
                switch( blendShapesARKit[i].blendShapeLocation )
                {
                    case ARKitBlendShapeLocation.MouthFunnel:
                        _avatar.SetMouthForm( 1 - blendShapesARKit[i].coefficient * 2f );
                        break;
                    case ARKitBlendShapeLocation.JawOpen:
                        _avatar.SetMouthOpen( blendShapesARKit[i].coefficient * 1.8f );
                        break;
                }
            }
        }

        // TODO @Choi 25.03.04 00:40
        // Body 트래킹이 안 먹힐 경우, 이하의 내용은 주입할 전략
        // 리팩터링 대상(Must)
        private void UpdateBodyBlendShape( ARFace arFace )
        {
            // Face Tracking을 통한 머리 회전 값 가져오기
            var headRotation = arFace.transform.rotation;

            // 오일러 각도로 변환 (Unity 좌표계 기준)
            var eulerRotation = headRotation.eulerAngles;

            // X, Y, Z 각도를 Live2D 아바타의 BodyAngleX, Y, Z로 변환
            var bodyAngleX = Mathf.Clamp( eulerRotation.x, -30f, 30f );
            var bodyAngleY = Mathf.Clamp( eulerRotation.y, -40f, 40f );
            var bodyAngleZ = Mathf.Clamp( eulerRotation.z, -20f, 20f );

            // 아바타 적용
            _avatar.SetBodyAngleX( bodyAngleX );
            _avatar.SetBodyAngleY( bodyAngleY );
            _avatar.SetBodyAngleZ( bodyAngleZ );

            // TODO 로그 출력 (디버깅용) @Choi 25.03.04 00:40
            Debug.Log( $"Head Rotation: X={bodyAngleX}, Y={bodyAngleY}, Z={bodyAngleZ}" );
            _logHeader.text = $"Head Rotation: X={bodyAngleX}, Y={bodyAngleY}, Z={bodyAngleZ}";
        }

        /// <summary>
        /// 얼굴 각도를 정규화하는 함수
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        private float NormalizeAngle( float angle )
        {
            if( angle > 180 )
            {
                return angle - 360;
            }
            return angle;
        }
    }
}
