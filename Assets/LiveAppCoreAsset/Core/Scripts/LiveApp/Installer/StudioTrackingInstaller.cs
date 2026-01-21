using StudioTrackingSDK.Domain;
using StudioTrackingSDK.Infrastructure;
using UnityEngine;
using Zenject;

namespace LiveAppCore.Installer
{
    public class StudioTrackingInstaller : MonoInstaller
    {
        [SerializeField] private ARKitFaceTrakingController _arFaceTracking;
        [SerializeField] private ARKitEyeTrackingController _arEyeTracking;

        public override void InstallBindings()
        {
            Container
                .Bind<IFaceTrackingDomain>()
                .FromInstance( _arFaceTracking )
                .AsSingle();
            Container
                .Bind<IEyeTrackingDomain>()
                .FromInstance( _arEyeTracking )
                .AsSingle();
        }
    }
}
