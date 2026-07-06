using StudioResourceSDK.Domain;
using StudioResourceSDK.Infrastructure;
using Zenject;


namespace StudioResourceSDK.Application
{
    public class SystemResourceSDKInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container
                .Bind<IResourceConfigContext>()
                .To<ResourceConfigContext>()
                .AsSingle();

            Container
                .Bind<IResourceTableContext>()
                .To<ResourceTableContext>()
                .AsSingle();

            Container
                .Bind<IResourceConfigParseDomain>()
                .To<ResourceConfigParser>()
                .AsSingle();
        }
    }
}