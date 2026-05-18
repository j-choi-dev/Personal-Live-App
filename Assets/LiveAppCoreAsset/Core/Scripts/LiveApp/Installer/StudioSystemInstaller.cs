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
                .Bind<IFileSystemDomain>()
                .To<FileSystemInfrastructure>()
                .AsSingle();
        }
    }
}
