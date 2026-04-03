using Source.Infrastructure.MVVM.Factory;
using VContainer;
using VContainer.Unity;

namespace Source.Infrastructure.MVVM.Installer
{
    public class MVVMInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.Register<ScreenFactory>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<ScreenRegistry>(Lifetime.Singleton).AsImplementedInterfaces();
        }
    }   
}