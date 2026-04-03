using VContainer;

namespace Source.Infrastructure.FSM.Factory
{
    public class ProjectStateFactory : IProjectStateFactory
    {
        private readonly IObjectResolver _objectResolver;

        public ProjectStateFactory(IObjectResolver objectResolver)
        {
            _objectResolver = objectResolver;
        }

        public TState CreateState<TState>() where TState : class, IProjectState
        {
            return _objectResolver.Resolve<TState>();
        }
    }
}