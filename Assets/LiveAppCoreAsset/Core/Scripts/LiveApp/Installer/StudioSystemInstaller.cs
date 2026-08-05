using LiveAppCore.Google.Application;
using LiveAppCore.Google.Domain;
using LiveAppCore.Google.Infrastructure;
using StudioSystemSDK.Application;
using StudioSystemSDK.Context;
using StudioSystemSDK.Domain;
using StudioSystemSDK.Infrastructure;
using UnityEngine;
using Zenject;

namespace LiveAppCore.Installer
{
    public class StudioSystemInstaller : MonoInstaller
    {
        [SerializeField] private iOSSigninInfrastructure _signInInfrastructure;
        [SerializeField] private CryptoKeySetting _cryptoKeySetting;
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
                .Bind<ICryptoContext>()
                .To<CryptoContext>()
                .AsSingle();
            Container
                .Bind<IObsAgentContext>()
                .To<ObsAgentContext>()
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
                .Bind<IObsAgentControlDomain>()
                .To<ObsAgentController>()
                .AsSingle();
            Container
                .Bind<IGoogleAuthTokenDomain>()
#if (UNITY_IOS || UNITY_IPHONE) && !UNITY_EDITOR
                .To<iOSAuthTokenInfrastructure>()
#else
                .To<StandaloneAuthTokenInfrastructure>()
#endif
                .AsSingle();

            Container
                .Bind<ICryptoProcessDomain>()
                .To<AESCryptoProcessor>()
                .AsSingle();
            Container
                .Bind<INativeSigninDomain>()
                .FromInstance( _signInInfrastructure )
                .AsSingle();
            Container
                .Bind<ICryptoKeySettingDomain>()
                .FromInstance(_cryptoKeySetting)
                .AsSingle();
        }
    }
}
