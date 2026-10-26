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
        [SerializeField] private int _bodyJointIndex;

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
                _bodyManager.subsystem.running == false )
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

        private void ProcessBody(ARHumanBody body)
        {
            if (body == null || !body.joints.IsCreated)
            {
                return;
            }

            if (_bodyJointIndex < 0 || _bodyJointIndex >= body.joints.Length)
            {
                return;
            }

            var joint = body.joints[_bodyJointIndex];
            if ( joint.tracked == false )
            {
                return;
            }

            UpdateBodyDirection(joint);
        }

        private void UpdateBodyDirection(XRHumanBodyJoint joint)
        {
            var euler = joint.anchorPose.rotation.eulerAngles;

            var x = Mathf.DeltaAngle(0f, euler.x);
            var y = Mathf.DeltaAngle(0f, euler.y);
            var z = Mathf.DeltaAngle(0f, euler.z);

            _avatar.SetBodyAngleX(x);
            _avatar.SetBodyAngleY(y);
            _avatar.SetBodyAngleZ(z);
        }
    }
}
