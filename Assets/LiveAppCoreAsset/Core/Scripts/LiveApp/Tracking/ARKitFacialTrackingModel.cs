using StudioCharacterSDK.Domain;
using StudioResourceSDK.Application;
using StudioTrackingSDK.Application;
using System;
using System.Diagnostics;
using UniRx;

namespace LiveApp
{
    public interface IARKitFacialTrackingModel
    {
        bool IsAbleTracking { get; }
        float Intensity { get; }

        void SetAbleTracking( bool isOK );

        void SetIntensity( float intensity );
    }
    public class ARKitFacialTrackingModel : IARKitFacialTrackingModel, IDisposable
    {
        private IFaceTrackingContext _faceTrackingContext;
        private IEyeTrackingContext _eyeTrackingContext;
        private ISceneResourceListContext _sceneResourceListContext;

        private IFacialData _currentCharacter = null;
        private CompositeDisposable _disposables = new CompositeDisposable();


        public ARKitFacialTrackingModel( IFaceTrackingContext faceTrackingContext,
            IEyeTrackingContext eyeTrackingContext,
            ISceneResourceListContext sceneResourceListContext )
        {
            _faceTrackingContext = faceTrackingContext;
            _eyeTrackingContext = eyeTrackingContext;
            _sceneResourceListContext = sceneResourceListContext;

            SubscribeTrackingData();

            // TODO 리팩터링 대상!!! @Choi 26.07.29
            _sceneResourceListContext.OnCurrentCharacterChanged
                .Subscribe( arg =>
                {
                    var target = arg as IFacialData;
                    if( target == null )
                    {
                        UnityEngine.Debug.LogError( $"Invalid Resource ; Not Exist Ficial Component({arg.ID})" );
                        _faceTrackingContext.SetCharacterID( string.Empty );
                        _faceTrackingContext.SetIsActive( false );
                        _currentCharacter = null;
                        return;
                    }
                    _faceTrackingContext.SetCharacterID( arg.ID );
                    _faceTrackingContext.SetIsActive( true );

                    _eyeTrackingContext.SetCharacterID( arg.ID );
                    _eyeTrackingContext.SetIsActive( true );
                    _currentCharacter = target;
                } )
                .AddTo( _disposables );
        }

        public bool IsAbleTracking => _faceTrackingContext.IsActive;

        public float Intensity { get; private set; }

        public void Dispose()
        {
            _disposables.Dispose();
            _disposables = null;
        }

        public void SetAbleTracking( bool isOK )
            => _faceTrackingContext.SetIsActive(isOK);

        public void SetIntensity( float intensity )
            => Intensity = intensity;

        private void SubscribeTrackingData()
        {
            UnityEngine.Debug.Log( "SubscribeTrackingData" );
            _faceTrackingContext.OnFaceAngleX
                .Subscribe( val => _currentCharacter.SetFaceAngleX( val ) )
                .AddTo( _disposables );
            _faceTrackingContext.OnFaceAngleY
                .Subscribe( val => _currentCharacter.SetFaceAngleY( val ) )
                .AddTo( _disposables );
            _faceTrackingContext.OnFaceAngleZ
                .Subscribe( val => _currentCharacter.SetFaceAngleZ( val ) )
                .AddTo( _disposables );

            _eyeTrackingContext.OnEyeBlinkLeft
                .Subscribe( val => _currentCharacter.SetEyeBlinkLeft( val ) )
                .AddTo( _disposables );
            _eyeTrackingContext.OnEyeBlinkRight
                .Subscribe( val => _currentCharacter.SetEyeBlinkRight( val ) )
                .AddTo( _disposables );
            _eyeTrackingContext.OnEyeBallAngleX
                .Subscribe( val => _currentCharacter.SetEyeBallAngleX( val ) )
                .AddTo( _disposables );
            _eyeTrackingContext.OnEyeBallAngleY
                .Subscribe( val => _currentCharacter.SetEyeBallAngleY( val ) )
                .AddTo( _disposables );
        }
    }
}
