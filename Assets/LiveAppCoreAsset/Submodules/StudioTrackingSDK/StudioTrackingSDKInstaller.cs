using StudioTrackingSDK.Application;
using StudioTrackingSDK.Domain;
using StudioTrackingSDK.Infrastructure;
using UnityEngine;
using Zenject;


namespace StudioResourceSDK.Application
{
    public class StudioTrackingSDKInstaller : MonoInstaller
    {
        [SerializeField] private ARKitFaceTraker _faceTracker = null;
        [SerializeField] private ARKitEyeTracker _eyeTracker = null;
        public override void InstallBindings()
        {
            Container
                .Bind<IFaceTrackingContext>()
                .To<FaceTrackingContext>()
                .AsSingle();

            Container
                .Bind<IFaceTrackingDomain>()
                .FromInstance( _faceTracker );
            Container
                .Bind<IEyeTrackingDomain>()
                .FromInstance( _eyeTracker );
        }
    }
}
