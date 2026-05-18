using LiveAppUI.Presenter;
using LiveAppUI.View;
using UnityEngine;
using Zenject;

namespace LiveAppCore.Installer
{
    public class SystemMenuInstaller : MonoInstaller
    {
        [SerializeField] private SystemMenuView _systemMenuView = null;

        public override void InstallBindings()
        {
            Container
                .Bind<ISystemMenuView>()
                .FromInstance( _systemMenuView )
                .AsSingle();
        }
    }
}
