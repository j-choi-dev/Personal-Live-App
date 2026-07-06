using LiveAppCore.Google.Application;
using LiveAppCore.Google.Domain;
using LiveAppCore.Google.Infrastructure;
using StudioSystemSDK.Application;
using StudioSystemSDK.Domain;
using StudioSystemSDK.Infrastructure;
using Zenject;

namespace LiveAppCore.Installer
{
    public class StudioSystemInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container
                .Bind<IFileSystemContext>()
                .To<FileSystemContext>()
                .AsSingle();
            Container
                .Bind<IAuthInfoContext>()
                .To<AuthInfoContext>()
                .AsSingle();

            Container
                .Bind<IFileSystemDomain>()
                .To<FileSystemInfrastructure>()
                .AsSingle();
            Container
                .Bind<IFileSerializeDomain>()
                .To<FileSerializer>()
                .AsSingle();
            Container
                .Bind<IGoogleAuthInfoStorage>()
                .To<GoogleAuthInfoStorage>()
                .AsSingle();
            Container
                .Bind<IGoogleAuthTokenDomain>()
                .To<GoogleAuthTokenInfrastructure>()
                .AsSingle();
        }
    }
}
