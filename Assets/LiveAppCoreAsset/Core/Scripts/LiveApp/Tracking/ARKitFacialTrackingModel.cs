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
        private ISceneResourceListContext _sceneResourceListContext;
        private IFacialData _currentCharacter = null;

        private CompositeDisposable _disposables = new CompositeDisposable();


        public ARKitFacialTrackingModel( IFaceTrackingContext faceTrackingContext,
            ISceneResourceListContext sceneResourceListContext )
        {
            _faceTrackingContext = faceTrackingContext;
            _sceneResourceListContext = sceneResourceListContext;

            SubscribeTrackingData();

            _sceneResourceListContext.OnCurrentCharacterChanged
                .Subscribe( arg =>
                {
                    var target = arg as IFacialData;
                    if( target == null )
                    {
                        UnityEngine.Debug.LogError( $"Invalid Resource ; Not Exist Ficial Component({arg.ID})" );
                    }
                    _faceTrackingContext.SetCharacterID( arg.ID );
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
        }
    }
}
