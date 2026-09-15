using LiveAppCore;
using StudioCommonSDK.Domain;
using StudioCommonSDK.Infrastructure;
using UnityEngine;
using Zenject;

namespace StudioCommonSDK.Application
{
    public class StudioCommonSDKInstaller : MonoInstaller
    {
        [SerializeField] private SpawnPivotTransform _objectPivot = null;
        [SerializeField] private SpawnPivotTransform _backGroundPivot = null;
        public override void InstallBindings()
        {
            Container
                .Bind<ISpawnPivotTransform>()
                .WithId( SpawnPivotId.Object )
                .FromInstance( _objectPivot );
            Container
                .Bind<ISpawnPivotTransform>()
                .WithId( SpawnPivotId.Sprite )
                .FromInstance( _backGroundPivot );
        }
    }
}
