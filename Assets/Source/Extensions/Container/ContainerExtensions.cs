using VContainer;
using VContainer.Unity;

namespace Source.Extensions
{
    public static class ContainerExtensions
    {
        public static void Install<TInstaller>(this IContainerBuilder builder) where TInstaller : IInstaller, new()
        {
            TInstaller installer = new TInstaller();
            installer.Install(builder);
        }
    }
}