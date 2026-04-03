using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace Source.Infrastructure.MVVM
{
    public interface IScreenView
    {
        event Action OnShowStarted;
        event Action OnShowCompleted;
        event Action OnHideStarted;
        event Action OnHideCompleted;
        void SetupShowAnimation();
        void SetupHideAnimation();
        void Initialize(IScreenViewModel viewModel);
        UniTask Show();
        UniTask Hide();
        void Dispose();
    }
}