using Cysharp.Threading.Tasks;
using Source.Infrastructure.FSM;
using Source.Infrastructure.MVVM.UI;

namespace Source.Boot
{
    public sealed class BootState : IProjectState
    {
        private readonly IUIService _uiService;
        private readonly GlobalObjectProvider _globalObjectProvider;

        public BootState(IUIService uiService, GlobalObjectProvider globalObjectProvider)
        {
            _uiService = uiService;
            _globalObjectProvider = globalObjectProvider;
        }

        public async UniTask Enter()
        {
            _uiService.ShowScreen(UIScreenType.MainMenu, _globalObjectProvider.CanvasOverlay).Forget();
        }

        public async UniTask Exit()
        {

        }
    }
}