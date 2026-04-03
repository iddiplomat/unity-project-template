using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Source.Infrastructure.MVVM.Factory
{
    public interface IScreenFactory
    {
        UniTask<TView> CreateScreen<TView, TViewModel>(string address, Transform parent)
            where TView : ScreenView<TViewModel>
            where TViewModel : IScreenViewModel;

        UniTask<IScreenView> CreateScreen(Type viewType, Type viewModelType, string address, Transform parent);

        UniTask DisposeScreen<TView, TViewModel>()
            where TView : ScreenView<TViewModel>
            where TViewModel : IScreenViewModel;

        UniTask DisposeScreen(Type viewType, Type viewModelType);
    }
}