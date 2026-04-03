using Cysharp.Threading.Tasks;
using Source.Infrastructure.FSM;
using Source.Infrastructure.FSM.Factory;
using VContainer.Unity;

namespace Source.Boot
{
    public class EntryPoint : IStartable
    {
        private readonly IProjectStateMachine _projectStateMachine;
        private readonly IProjectStateFactory _stateFactory;

        public EntryPoint(IProjectStateMachine projectStateMachine, IProjectStateFactory stateFactory)
        {
            _projectStateMachine = projectStateMachine;
            _stateFactory = stateFactory;
        }

        public void Start()
        {
            AddStates();

            _projectStateMachine.Enter<BootState>().Forget();
        }

        private void AddStates()
        {
            _projectStateMachine.AddState<BootState>(_stateFactory.CreateState<BootState>());
        }
    }
}