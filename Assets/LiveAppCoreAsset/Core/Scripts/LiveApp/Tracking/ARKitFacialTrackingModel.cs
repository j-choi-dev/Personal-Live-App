using StudioResourceSDK.Application;
using StudioTrackingSDK.Application;
using System;
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

        private CompositeDisposable _disposables = new CompositeDisposable();


        public ARKitFacialTrackingModel( IFaceTrackingContext faceTrackingContext,
            ISceneResourceListContext sceneResourceListContext )
        {
            _faceTrackingContext = faceTrackingContext;
            _sceneResourceListContext = sceneResourceListContext;

            _sceneResourceListContext.OnCurrentCharacterChanged
                .Subscribe( arg => _faceTrackingContext.SertCharacterID( arg.ID ) )
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
    }
}
