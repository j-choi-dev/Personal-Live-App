using StudioRendererSDK.Application;
using StudioRendererSDK.Domain;
using StudioRendererSDK.Infrastructure;
using UnityEngine;
using Zenject;

namespace LiveAppCore.Installer
{
    public class StudioRendererSDKInstaller : MonoInstaller
    {
        [SerializeField] private ObsRenderTextureVideoSource _obsRenderSource = null;
        [SerializeField] private ObsWebRtcSenderSession _obsRenderSendSession = null;

        public override void InstallBindings()
        {
            Container
                .Bind<IObsAgentContext>()
                .To<ObsAgentContext>()
                .AsSingle();
            Container
                .Bind<IRenderSendContext>()
                .To<RenderSendContext>()
                .AsSingle();

            Container
                .Bind<IObsAgentControlDomain>()
                .To<ObsAgentController>()
                .AsSingle();
            Container
                .Bind<IWebRtcSenderSessionDomain>()
                .FromInstance( _obsRenderSendSession )
                .AsSingle();
            Container
                .Bind<IObsVideoSource>()
                .FromInstance( _obsRenderSource )
                .AsSingle();
            //Container
            //    .Bind( typeof(IObsVideoSource),
            //        typeof( IWebRtcSenderSessionDomain)
            //    )
            //    .FromInstance( _obsRenderSource )
            //    .AsSingle();
        }
    }
}