using StudioCommonSDK.Domain;
using StudioCommonSDK.Infrastructure;
using UnityEngine;
using Zenject;

namespace StudioCommonSDK.Application
{
    public class StudioCommonSDKInstaller : MonoInstaller
    {
        [SerializeField] private SpawnPivotTransform _pivot = null;
        public override void InstallBindings()
        {
            Container
                .Bind<ISpawnPivotTransform>()
                .FromInstance( _pivot );
        }
    }
}
