using StudioTrackingSDK.Domain;
using System;

namespace StudioTrackingSDK.Application
{
    public class FaceTrackingContext : IFaceTrackingContext
    {
        private IFaceTrackingDomain _faceTrackingDomain;

        public IObservable<float> OnFaceAngleX => _faceTrackingDomain.OnFaceAngleX;
        public IObservable<float> OnFaceAngleY => _faceTrackingDomain.OnFaceAngleY;
        public IObservable<float> OnFaceAngleZ => _faceTrackingDomain.OnFaceAngleZ;

        public bool IsActive => _faceTrackingDomain.IsActive;

        public string CurrentSelectedCharacter { get; private set; }

        public FaceTrackingContext( IFaceTrackingDomain faceTrackingDomain )
        {
            _faceTrackingDomain = faceTrackingDomain;
        }

        public void SertCharacterID( string id )
        {
            CurrentSelectedCharacter = id;
            UnityEngine.Debug.Log( $"CurrentSelectedCharacter = {CurrentSelectedCharacter}" );
        }

        public void SetIsActive( bool isValue )
        {
            _faceTrackingDomain.SetIsActive( isValue );
        }
    }
}
