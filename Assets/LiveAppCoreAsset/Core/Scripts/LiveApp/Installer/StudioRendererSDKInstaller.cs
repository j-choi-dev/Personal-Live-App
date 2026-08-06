using StudioRendererSDK.Application;
using StudioRendererSDK.Domain;
using StudioRendererSDK.Infrastructure;
using UnityEngine;
using Zenject;

namespace LiveAppCore.Installer
{
    public class StudioRendererSDKInstaller : MonoInstaller
    {
        [SerializeField] private ObsRenderOutput _obsRenderOutput = null;
        public override void InstallBindings()
        {
            Container
            .Bind<IObsAgentContext>()
                .To<ObsAgentContext>()
                .AsSingle();

            Container
                .Bind<IObsAgentControlDomain>()
                    .To<ObsAgentController>()
                    .AsSingle();

            Container.Bind<IObsVideoSource>()
                .FromInstance( _obsRenderOutput )
                .AsSingle();
        }
    }
}