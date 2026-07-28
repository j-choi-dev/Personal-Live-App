using Live2D.Cubism.Core;
using StudioCharacterSDK.Domain;
using System;
using System.Linq;
using UniRx;
using UnityEngine;

namespace StudioCharacterSDK.Infrastructure
{
    public class StudioAvatar : MonoBehaviour, ICharacter, IFacialData
    {
        [SerializeField] private AvatarParameterPair _pair = null;
        [SerializeField] private CubismModel _avatar = null;
        [SerializeField] private CubismMoc _moc;
        private bool _isInitialized = false;

        // Live2D 파라미터 멤버변수
        private CubismParameter _faceAngleX = null;
        private CubismParameter _faceAngleY = null;
        private CubismParameter _faceAngleZ = null;
        private CubismParameter _bodyAngleX = null;
        private CubismParameter _bodyAngleY = null;
        private CubismParameter _bodyAngleZ = null;
        private CubismParameter _leftEyeBlink = null;
        private CubismParameter _rightEyeBlink = null;
        private CubismParameter _eyeBallX = null;
        private CubismParameter _eyeBallY = null;
        private CubismParameter _mouthForm = null;
        private CubismParameter _mouthOpenY = null;

        // 얼굴 회전값 멤버변수
        private float _updateFaceAngleX;
        private float _updateFaceAngleY;
        private float _updateFaceAngleZ;

        // 눈동자 정보 멤버변수
        private float _updateLeftEye;
        private float _updateRightEye;

        // 눈동자 X 값 멤버변수
        private float _updateEyeballX;
        // 눈동자 Y 값 멤버변수
        private float _updateEyeballY;

        // 입술 정보 멤버변수
        private float _updateMouthForm;
        private float _updateMouthOpen;

        // 몸 회전값 멤버변수
        private float _updateBodyAngleX;
        private float _updateBodyAngleY;
        private float _updateBodyAngleZ;

        private float _updateMouthOpenY;

        public bool IsInitialized => _isInitialized;

        public string ID { get; private set; }

        public float FaceAngleX => _updateFaceAngleX;

        public float FaceAngleY => _updateFaceAngleY;

        public float FaceAngleZ => _updateFaceAngleZ;

        private Subject<float> _onChangeFaceAngleX = new Subject<float>();
        public IObservable<float> OnChangeFaceAngleX => _onChangeFaceAngleX;

        private Subject<float> _onChangeFaceAngleY = new Subject<float>();
        public IObservable<float> OnChangeFaceAngleY => _onChangeFaceAngleY;


        private Subject<float> _onChangeFaceAngleZ = new Subject<float>();
        public IObservable<float> OnChangeFaceAngleZ => _onChangeFaceAngleZ;

        private void Awake()
        {
            Init();
        }

        public void Init()
        {
            //_avatar.SetCubismMoc( _moc ); // TODO 복원 대상 @Choi 26.07.08
            InitCubismParameter();
            _isInitialized = true;
        }

        /// <summary>
        /// 아바타의 모션 정보를 멤버 변수에 대입
        /// </summary>
        /// <param name="model"></param>
        public void InitCubismParameter()
        {
            _faceAngleX = _avatar.Parameters.First( arg => arg.Id == _pair.ParameterPairs[( int )AvatarPartsParameter.FaceAngle_X].parameter.Id );
            _faceAngleY = _avatar.Parameters.First( arg => arg.Id == _pair.ParameterPairs[( int )AvatarPartsParameter.FaceAngle_Y].parameter.Id );
            _faceAngleZ = _avatar.Parameters.First( arg => arg.Id == _pair.ParameterPairs[( int )AvatarPartsParameter.FaceAngle_Z].parameter.Id );

            _leftEyeBlink = _avatar.Parameters.First( arg => arg.Id ==_pair.ParameterPairs[( int )AvatarPartsParameter.LeftEyeBlink].parameter.Id );
            _rightEyeBlink = _avatar.Parameters.First( arg => arg.Id == _pair.ParameterPairs[( int )AvatarPartsParameter.RightEyeBlink].parameter.Id );

            _eyeBallX = _avatar.Parameters.First( arg => arg.Id == _pair.ParameterPairs[( int )AvatarPartsParameter.EyeBallX].parameter.Id );
            _eyeBallY = _avatar.Parameters.First( arg => arg.Id == _pair.ParameterPairs[( int )AvatarPartsParameter.EyeBallY].parameter.Id );

            _bodyAngleX = _avatar.Parameters.First( arg => arg.Id == _pair.ParameterPairs[( int )AvatarPartsParameter.BodyAngle_X].parameter.Id );
            _bodyAngleY = _avatar.Parameters.First( arg => arg.Id == _pair.ParameterPairs[( int )AvatarPartsParameter.BodyAngle_Y].parameter.Id );
            _bodyAngleZ = _avatar.Parameters.First( arg => arg.Id == _pair.ParameterPairs[( int )AvatarPartsParameter.BodyAngle_Z].parameter.Id );

            _mouthOpenY = _avatar.Parameters.First( arg => arg.Id == _pair.ParameterPairs[( int )AvatarPartsParameter.MouthOpen_Y].parameter.Id );
        }

        public void SetFaceAngleX( float value )
        {
            _faceAngleX.Value = value;
            UnityEngine.Debug.Log( $"_faceAngleX = {_faceAngleX.Value}" );
        }
        public void SetFaceAngleY( float value )
        {
            _faceAngleY.Value = value;
            UnityEngine.Debug.Log( $"_faceAngleY = {_faceAngleY.Value}" );
        }
        public void SetFaceAngleZ( float value )
        {
            _faceAngleZ.Value = value;
            UnityEngine.Debug.Log( $"_faceAngleZ = {_faceAngleZ.Value}" );
        }

        public void SetEyeBlinkLeft( float value ) => _updateLeftEye = value;
        public void SetEyeBlinkRight( float value ) => _updateRightEye = value;
        public void SetEyeLookHorizontal( float value ) => _updateEyeballX = value;
        public void SetEyeLookVertical( float value ) => _updateEyeballY = value;

        public void SetMouthForm( float value ) => _updateMouthForm = value;
        public void SetMouthOpen( float value ) => _updateMouthOpen = value;

        public void SetBodyAngleX( float value ) => _updateBodyAngleX = value;
        public void SetBodyAngleY( float value ) => _updateBodyAngleY = value;
        public void SetBodyAngleZ( float value ) => _updateBodyAngleZ = value;

        public void SetMouthParamY( float value ) => _updateMouthOpenY = value;

        private void LateUpdate()
        {
            if( _isInitialized == false )
            {
                return;
            }
            _faceAngleX.Value = _updateFaceAngleX;
            _faceAngleY.Value = _updateFaceAngleY;
            _faceAngleZ.Value = _updateFaceAngleZ;

            _leftEyeBlink.Value = _updateLeftEye;
            _rightEyeBlink.Value = _updateRightEye;

            _eyeBallX.Value = _updateEyeballX;
            _eyeBallY.Value = _updateEyeballY;

            _bodyAngleX.Value = _updateBodyAngleX;
            _bodyAngleY.Value = _updateBodyAngleY;
            _bodyAngleZ.Value = _updateBodyAngleZ;

            _mouthOpenY.Value = _updateMouthOpenY;
        }

        public void SetID( string id )
            => ID = id;
    }
}
