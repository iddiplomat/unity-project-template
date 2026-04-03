using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Source.Infrastructure.MVVM.UI
{
    public interface IUIService
    {
        UniTask<IScreenView> ShowScreen<TView, TViewModel>(
            UIScreenType type,
            Transform parent)
            where TViewModel : IScreenViewModel
            where TView : ScreenView<TViewModel>;

        UniTask<IScreenView> ShowScreen(
            UIScreenType type,
            Transform parent);

        UniTask HideScreen<TView, TViewModel>()
            where TViewModel : IScreenViewModel
            where TView : ScreenView<TViewModel>;

        UniTask HideScreen(UIScreenType type);
    }
}