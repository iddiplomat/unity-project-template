using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Source.Infrastructure.MVVM;
using Source.Infrastructure.MVVM.Factory;
using UnityEngine;

namespace Source.Infrastructure.MVVM.UI
{
    public class UIService : IUIService
    {
        private readonly IScreenFactory _screenFactory;
        private readonly IScreenRegistry _screenRegistry;

        public UIService(IScreenFactory screenFactory, IScreenRegistry screenRegistry)
        {
            _screenFactory = screenFactory;
            _screenRegistry = screenRegistry;
        }

        public async UniTask<IScreenView> ShowScreen<TView, TViewModel>(
            UIScreenType type,
            Transform parent)
            where TViewModel : IScreenViewModel
            where TView : ScreenView<TViewModel>
        {
            IScreenView view = await _screenFactory.CreateScreen<TView, TViewModel>(type.GetAddress(), parent);

            if (view != null)
            {
                view.SetupShowAnimation();
                view.SetupHideAnimation();
            }

            return view;
        }

        public async UniTask<IScreenView> ShowScreen(
            UIScreenType type,
            Transform parent)
        {
            var binding = _screenRegistry.GetBinding(type);
            IScreenView view = await _screenFactory.CreateScreen(
                binding.ViewType, binding.ViewModelType, type.GetAddress(), parent);

            if (view != null)
            {
                view.SetupShowAnimation();
                view.SetupHideAnimation();
            }

            return view;
        }

        public async UniTask HideScreen<TView, TViewModel>()
            where TViewModel : IScreenViewModel
            where TView : ScreenView<TViewModel>
        {
            await _screenFactory.DisposeScreen<TView, TViewModel>();
        }

        public async UniTask HideScreen(UIScreenType type)
        {
            var binding = _screenRegistry.GetBinding(type);
            await _screenFactory.DisposeScreen(binding.ViewType, binding.ViewModelType);
        }
    }
}