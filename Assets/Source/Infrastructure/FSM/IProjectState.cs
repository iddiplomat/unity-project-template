using Cysharp.Threading.Tasks;

namespace Source.Infrastructure.FSM
{
    public interface IProjectState
    {
        UniTask Enter();
        UniTask Exit();
    }
}