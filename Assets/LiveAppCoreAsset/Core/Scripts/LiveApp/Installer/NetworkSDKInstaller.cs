using StudioNetworkSDK.Application;
using StudioNetworkSDK.Domain;
using StudioNetworkSDK.Infrastructure;
using Zenject;

namespace LiveAppCore.Installer
{
    public class NetworkSDKInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container
                .Bind<INetworkRequestApplication>()
                .To<NetworkRequestApplication>()
                .AsSingle();

            Container
                .Bind<ISenderDomain>()
                .To<MqTTSender>()
                .AsSingle();

            Container
                .Bind<IReceiverDomain>()
                .To<MqTTReceiver>()
                .AsSingle();

            Container
                .Bind<IServerConfigDomain>()
                .To<ServerConfigData>()
                .AsSingle();
        }
    }
}
