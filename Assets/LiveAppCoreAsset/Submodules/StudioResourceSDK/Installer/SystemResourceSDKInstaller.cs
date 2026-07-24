using LiveAppCore.Google.Infrastructure;
using StudioResourceSDK.Domain;
using StudioResourceSDK.Infrastructure;
using StudioSystemSDK.Domain;
using StudioSystemSDK.Infrastructure;
using Zenject;


namespace StudioResourceSDK.Application
{
    public class SystemResourceSDKInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container
                .Bind<IResourceServerConfigContext>()
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
            Container
                .Bind<IResourceTableLoadDomain>()
#if !UNITY_EDITOR_WIN && (UNITY_IOS || UNITY_IPHONE) 
                .To<iOSGoogleSheetLoader>()
#elif UNITY_EDITOR_WIN || !(UNITY_IOS || UNITY_IPHONE)
                .To<StandaloneGoogleSheetLoader>()
#endif
                .AsSingle();
            Container
                .Bind<IResourceDataParseDomain>()
                .To<ResourceDataParser>()
                .AsSingle();
        }
    }
}