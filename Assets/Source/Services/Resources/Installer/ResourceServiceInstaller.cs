using Source.Services.Resources;
using VContainer;
using VContainer.Unity;

namespace Source.Services.Resources.Installer
{
    public sealed class ResourceServiceInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.Register<UnityAddressablesBackend>(Lifetime.Singleton).As<IAddressablesBackend>();
            builder.Register<ResourceService>(Lifetime.Singleton).AsImplementedInterfaces();
        }
    }
}
