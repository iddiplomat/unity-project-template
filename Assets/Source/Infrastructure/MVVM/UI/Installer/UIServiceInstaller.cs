using VContainer;
using VContainer.Unity;

namespace Source.Infrastructure.MVVM.UI.Installer
{
    public class UIServiceInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.Register<UIService>(Lifetime.Singleton).AsImplementedInterfaces();
        }
    }
}