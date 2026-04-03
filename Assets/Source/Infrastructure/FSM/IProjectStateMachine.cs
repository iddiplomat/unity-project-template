using Cysharp.Threading.Tasks;

namespace Source.Infrastructure.FSM
{
    public interface IProjectStateMachine
    {
        void AddState<TState>(TState state) where TState : IProjectState;
        UniTask Enter<TState>() where TState : IProjectState;
    }
}