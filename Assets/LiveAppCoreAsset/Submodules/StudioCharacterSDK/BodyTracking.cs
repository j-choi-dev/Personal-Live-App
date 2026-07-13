using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Linq;
using StudioCharacterSDK.Infrastructure;

namespace StudioTrackingSDK.Infrastructure
{
    public class BodyTracking : MonoBehaviour
    {
        [SerializeField] private ARHumanBodyManager _bodyManager;
        [SerializeField] private StudioAvatar _avatar;
        [SerializeField] private GameObject _flagARKit;
        [SerializeField] private GameObject _bodyFlag;

        private void Awake()
        {
            _flagARKit.SetActive( false );
            _bodyFlag.SetActive( false );

            var descriptors = new List<XRSessionSubsystemDescriptor>();
            SubsystemManager.GetSubsystemDescriptors( descriptors );

            foreach( var descriptor in descriptors )
            {
                Debug.Log( $"[XRSessionSubsystem] {descriptor.id}" );
            }

            // ARKit이 지원되는지 확인
            XRSessionSubsystem sessionSubsystem = null;
            foreach( var descriptor in descriptors )
            {
                if( descriptor.id == "ARKit-Session" )
                {
                    sessionSubsystem = descriptor.Create();
                    break;
                }
            }
            var bodyMessage1 = string.Empty;
            if( sessionSubsystem == null )
            {
                _flagARKit.SetActive( true );
            }

            var bodyMessage2 = string.Empty;
            if( _bodyManager == null ||
                _bodyManager.subsystem == null ||
                _bodyManager.subsystem.running )
            {
                _bodyFlag.SetActive( true );
            }
        }

        private void OnEnable()
        {
            _bodyManager.humanBodiesChanged += OnHumanBodiesChanged;
        }

        private void OnDisable()
        {
            _bodyManager.humanBodiesChanged -= OnHumanBodiesChanged;
        }

        private void OnHumanBodiesChanged( ARHumanBodiesChangedEventArgs args )
        {
            var bodyList = args.added;
            bodyList.AddRange( args.updated );
            bodyList.Distinct();
            foreach( var body in bodyList )
            {
                ProcessBody( body );
            }
        }

        private void ProcessBody( ARHumanBody body )
        {
            int hipsIndex = ( int )HumanBodyBones.Hips;
            if( hipsIndex < 0 || hipsIndex >= body.joints.Length )
            {
                return;
            }
            var hips = body.joints[hipsIndex];
            if( hips.tracked )
            {
                var posLog = $"Hips Position: {hips.anchorPose.position}\nHips Rotation: {hips.anchorPose.rotation.eulerAngles}";

                UpdateBodyDirection( hips );
            }
        }

        private void UpdateBodyDirection( XRHumanBodyJoint spine )
        {
            // Live2D 연동: BodyAngleX, Y, Z
            var bodyAngleX = spine.anchorPose.rotation.eulerAngles.x;
            var bodyAngleY = spine.anchorPose.rotation.eulerAngles.y;
            var bodyAngleZ = spine.anchorPose.rotation.eulerAngles.z;
            _avatar.SetBodyAngleX( bodyAngleX );
            _avatar.SetBodyAngleY( bodyAngleY );
            _avatar.SetBodyAngleZ( bodyAngleZ );
        }
    }
}
