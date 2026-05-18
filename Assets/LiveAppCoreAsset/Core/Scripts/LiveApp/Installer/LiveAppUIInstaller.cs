using LiveAppUI.Model;
using LiveAppUI.Presenter;
using LiveAppUI.View;
using UnityEngine;
using Zenject;

namespace LiveAppUI.Installer
{
    public class LiveAppUIInstaller : MonoInstaller
    {
        [SerializeField] private ServerModalView _serverModalView;
        [SerializeField] private RoomModalView _roomModalView;

        public override void InstallBindings()
        {
            Container
                .Bind<IServerModalView>()
                .FromInstance( _serverModalView );
            Container
                .Bind<IRoomModalView>()
                .FromInstance( _roomModalView );

            Container
                .Bind<ILogInModel>()
                .To<LogInModel>()
                .AsSingle();
        }
    }
}
