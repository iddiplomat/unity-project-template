using Source.Infrastructure.FSM.Factory;
using VContainer;
using VContainer.Unity;

namespace Source.Infrastructure.FSM.Installer
{
    public class ProjectStateMachineInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.Register<ProjectStateFactory>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<ProjectStateMachine>(Lifetime.Singleton).AsImplementedInterfaces();
        }
    }
}