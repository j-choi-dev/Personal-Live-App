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
        [SerializeField] private OVRLipSyncVowelSource _ovrLipsync = null;
        public override void InstallBindings()
        {
            Container
                .Bind<IFaceTrackingContext>()
                .To<FaceTrackingContext>()
                .AsSingle();
            Container
                .Bind<IEyeTrackingContext>()
                .To<EyeTrackingContext>()
                .AsSingle();
            Container
                .Bind<ILipSyncContext>()
                .To<LipSyncContext>()
                .AsSingle();

            Container
                .Bind<IFaceTrackingDomain>()
                .FromInstance( _faceTracker );
            Container
                .Bind<IEyeTrackingDomain>()
                .FromInstance( _eyeTracker );
            Container
                .Bind<ILipSyncDomain>()
                .FromInstance( _ovrLipsync );
        }
    }
}
