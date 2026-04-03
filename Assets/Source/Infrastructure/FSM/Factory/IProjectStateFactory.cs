namespace Source.Infrastructure.FSM.Factory
{
    public interface IProjectStateFactory
    {
        TState CreateState<TState>() where TState : class, IProjectState;
    }
}