using Source.Extensions;
using Source.Infrastructure.FSM.Installer;
using Source.Infrastructure.MVVM.Installer;
using Source.Infrastructure.MVVM.UI.Installer;
using Source.Services.Resources.Installer;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Source.Boot
{
    public class RootInstaller : LifetimeScope
    {
        [SerializeField] private Transform canvasOverlayParent;
        [SerializeField] private Transform canvasCameraParent;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<EntryPoint>();

            RegisterProjectStates(builder);
            RegisterGlobalObjectsProvider(builder);

            builder.Install<ResourceServiceInstaller>();
            builder.Install<MVVMInstaller>();
            builder.Install<UIServiceInstaller>();
            builder.Install<ProjectStateMachineInstaller>();
        }

        private void RegisterProjectStates(IContainerBuilder builder)
        {
            builder.Register<BootState>(Lifetime.Singleton);
        }

        private void RegisterGlobalObjectsProvider(IContainerBuilder builder)
        {
            GlobalObjectProvider globalObjectProvider = new GlobalObjectProvider();
            globalObjectProvider.Initialize(canvasOverlayParent, canvasCameraParent);
            builder.RegisterInstance<GlobalObjectProvider>(globalObjectProvider);
        }
    }
}