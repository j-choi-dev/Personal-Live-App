using Live2D.Cubism.Core;
using StudioCharacterSDK.Domain;
using System;
using System.Linq;
using UniRx;
using UnityEngine;

namespace StudioCharacterSDK.Infrastructure
{
    public class StudioAvatar : MonoBehaviour, ICharacter, IFacialData, ILipSyncData
    {
        [SerializeField] private AvatarParameterPair _pair = null;
        [SerializeField] private CubismModel _avatar = null;
        [SerializeField] private CubismMoc _moc;
        private bool _isInitialized = false;
        private LipSyncVowelData _lipSyncValue;

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

        public float EyeBallAngleX => _updateEyeballX;
        public float EyeBallAngleY => _updateEyeballY;
        public float EyeBlinkLeft => _updateLeftEye;
        public float EyeBlinkRight => _updateRightEye;


        private Subject<float> _onChangeEyeBallAngleX = new Subject<float>();
        public IObservable<float> OnChangeEyeBallAngleX => _onChangeEyeBallAngleX;


        private Subject<float> _onChangeEyeBallAngleY = new Subject<float>();
        public IObservable<float> OnChangeEyeBallAngleY => _onChangeEyeBallAngleY;


        private Subject<float> _onChangeEyeBlinkLeft= new Subject<float>();
        public IObservable<float> OnChangeEyeBlinkLeft => _onChangeEyeBlinkLeft;


        private Subject<float> _onChangeEyeBlinkRight = new Subject<float>();
        public IObservable<float> OnChangeEyeBlinkRight => _onChangeEyeBlinkRight;

        private Subject<LipSyncVowelData> _onChangeLipSync = new Subject<LipSyncVowelData>();
        public IObservable<LipSyncVowelData> OnChangeLipSync => _onChangeLipSync;

        public LipSyncVowelData LipSyncValue => _lipSyncValue;


        private Subject<float> _onChangeMouthForm = new Subject<float>();
        public IObservable<float> OnChangeMouthForm => _onChangeMouthForm;
        private Subject<float> _onChangeMouthOpen = new Subject<float>();
        public IObservable<float> OnChangeMouthOpen => _onChangeMouthOpen;

        private void Awake()
        {
            Init();
        }

        public void Init()
        {
            _isInitialized = false;

            if( _avatar == null )
            {
                Debug.LogError( $"[StudioAvatar] Init failed: _avatar is null. Name={name}", this );
                return;
            }

            if( _pair == null )
            {
                Debug.LogError( $"[StudioAvatar] Init failed: _pair is null. Name={name}", this );
                return;
            }

            if( _pair.ParameterPairs == null )
            {
                Debug.LogError( $"[StudioAvatar] Init failed: ParameterPairs is null. Name={name}", this );
                return;
            }

            try
            {
                InitCubismParameter();

                if( _faceAngleX == null ||
                    _faceAngleY == null ||
                    _faceAngleZ == null )
                {
                    return;
                }
                _isInitialized = true;
                Debug.Log( $"Init completed. FaceX={_faceAngleX.Id}, FaceY={_faceAngleY.Id}, FaceZ={_faceAngleZ.Id}", this );
            }
            catch( Exception exception )
            {
                Debug.LogError( $"Init exception. Name={name}, ParameterPairCount={_pair.ParameterPairs.Count}", this );

                Debug.LogException( exception, this );
            }
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

            _mouthForm = _avatar.Parameters.First( arg => arg.Id == _pair.ParameterPairs[ ( int )AvatarPartsParameter.MouthForm ].parameter.Id );
            //_mouthOpenY = _avatar.Parameters.First( arg => arg.Id == _pair.ParameterPairs[ ( int )AvatarPartsParameter.MouthOpen_Y ].parameter.Id );
        }

        public void SetFaceAngleX( float value )
        {
            _updateFaceAngleX = value;
            UnityEngine.Debug.Log( $"SetFaceAngleX :: _updateFaceAngleX = {_updateFaceAngleX}" );
        }
        public void SetFaceAngleY( float value )
        {
            _updateFaceAngleY = value;
            UnityEngine.Debug.Log( $"SetFaceAngleY :: _updateFaceAngleY = {_updateFaceAngleY}" );
        }
        public void SetFaceAngleZ( float value )
        {
            _updateFaceAngleZ = value;
            UnityEngine.Debug.Log( $"SetFaceAngleZ :: _updateFaceAngleZ = {_updateFaceAngleZ}" );
        }

        public void SetEyeBlinkLeft( float value )
        {
            _updateLeftEye = value;
            UnityEngine.Debug.Log( $"SetEyeBlinkLeft :: _updateLeftEye = {_updateLeftEye}" );
        }
        public void SetEyeBlinkRight( float value )
        {
            _updateRightEye = value;
            UnityEngine.Debug.Log( $"SetEyeBlinkRight :: _updateRightEye = {_updateRightEye}" );
        }
        public void SetEyeBallAngleX( float value )
        {
            _updateEyeballX = value;
            UnityEngine.Debug.Log( $"SetEyeBallAngleX :: _updateEyeballX = {_updateEyeballX}" );
        }
        public void SetEyeBallAngleY( float value )
        {
            _updateEyeballY = value;
            UnityEngine.Debug.Log( $"SetEyeBallAngleY :: _updateEyeballY = {_updateEyeballY}" );
        }
        public void SetMouthForm( float value )
        {
            float clampedValue = Mathf.Clamp(value, -1f, 1f);

            if( Mathf.Abs( _updateMouthForm - clampedValue ) < 0.0001f )
            {
                return;
            }

            _updateMouthForm = clampedValue;
            _onChangeMouthForm.OnNext( clampedValue );
        }

        public void SetMouthOpen( float value )
        {
            float clampedValue = Mathf.Clamp01(value);

            if( Mathf.Abs( _updateMouthOpen - clampedValue ) < 0.0001f )
            {
                return;
            }

            _updateMouthOpen = clampedValue;
            _onChangeMouthOpen.OnNext( clampedValue );
        }

        public void SetBodyAngleX( float value ) => _updateBodyAngleX = value;
        public void SetBodyAngleY( float value ) => _updateBodyAngleY = value;
        public void SetBodyAngleZ( float value ) => _updateBodyAngleZ = value;

        private void ApplyLipSyncValue( LipSyncVowelData value )
        {
            // E, I가 강하면 양수 방향, O, U가 강하면 음수 방향, A는 형태상 중립이므로 포함X
            float mouthForm = (value.E + value.I) - (value.O + value.U);

            // 정규화된 모음 중 가장 강한 값을 사용. params 배열 할당을 피하기 위해 순차적으로 계산.
            float mouthOpen = value.A;
            mouthOpen = Mathf.Max( mouthOpen, value.E );
            mouthOpen = Mathf.Max( mouthOpen, value.I );
            mouthOpen = Mathf.Max( mouthOpen, value.O );
            mouthOpen = Mathf.Max( mouthOpen, value.U );

            SetMouthForm( mouthForm );
            SetMouthOpen( mouthOpen );
        }
        private bool _initializationErrorReported;
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

            _mouthForm.Value = _updateMouthForm;
        }

        public void SetID( string id )
            => ID = id;

        public void SetLipSync( LipSyncVowelData value )
        {
            _lipSyncValue = value;

            ApplyLipSyncValue( value );
            _onChangeLipSync.OnNext( value );
        }
    }
}
