using LiveApp;
using LiveAppUI.Model;
using LiveAppUI.Presenter;
using LiveAppUI.View;
using StudioResourceSDK.Application;
using UnityEngine;
using Zenject;

namespace LiveAppUI.Installer
{
    public class LiveAppUIInstaller : MonoInstaller
    {
        [SerializeField] private MainMenuView _mainMenuView;
        [SerializeField] private ResourceMenuView _resourceMenuView;
        [SerializeField] private ServerModalView _serverModalView;
        [SerializeField] private RoomModalView _roomModalView;
        [SerializeField] private ResourceListView _resourceListView;
        [SerializeField] private ConfigMenuView _configMenuView;

        public override void InstallBindings()
        {
            ViewBinding();
            ModelBinding();
        }

        private void ViewBinding()
        {
            Container
                .Bind<IMainMenuView>()
                .FromInstance( _mainMenuView );
            Container
                .Bind<IResourceMenuView>()
                .FromInstance( _resourceMenuView );
            Container
                .Bind<IServerModalView>()
                .FromInstance( _serverModalView );
            Container
                .Bind<IRoomModalView>()
                .FromInstance( _roomModalView );
            Container
                .Bind<IResourceListView>()
                .FromInstance( _resourceListView );
            Container
                .Bind<IConfigMenuView>()
                .FromInstance( _configMenuView );
        }

        private void ModelBinding()
        {
            Container
                .Bind<ILogInModel>()
                .To<LogInModel>()
                .AsSingle();

            Container
                .Bind<IOAuthTokenModel>()
                .To<OAuthTokenModel>()
                .AsSingle();

            Container
                .Bind<IResourceListModel>()
                .To<ResourceListModel>()
                .AsSingle();

            Container
                .Bind<IObsAgentModel>()
                .To<ObsAgentModel>()
                .AsSingle();

            Container
                .Bind<IARKitFacialTrackingModel>()
                .To<ARKitFacialTrackingModel>()
                .AsSingle()
                .NonLazy();

            Container
                .Bind<ILipSyncModel>()
                .To<LipSyncModel>()
                .AsSingle()
                .NonLazy();
        }
    }
}
