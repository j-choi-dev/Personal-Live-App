using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace LiveApp
{
    public interface IARKitFacialTrackingModel
    {
        bool IsAbleTracking { get; }
        bool IsExistManager { get; }
        float Intensity { get; }

        void SetAbleTracking(bool isOK);
        void SetARFaceManager(ARFaceManager manager);

        void SetIntensity(float intensity);
    }
    public class ARKitFacialTrackingModel : IARKitFacialTrackingModel
    {
        private ARFaceManager _faceManager; // ???? ????

        public ARKitFacialTrackingModel( ARFaceManager faceManager )
        {
            _faceManager = faceManager;
        }

        public bool IsAbleTracking { get; private set; }

        public bool IsExistManager => _faceManager != null;

        public float Intensity { get; private set; }

        public void SetAbleTracking( bool isOK )
            => IsAbleTracking = isOK;

        public void SetARFaceManager( ARFaceManager manager )
            => _faceManager = manager;

        public void SetIntensity( float intensity )
            => Intensity = intensity;
    }
}
