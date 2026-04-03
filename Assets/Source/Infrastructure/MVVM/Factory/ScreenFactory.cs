using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Source.Services.Resources;
using UnityEngine;
using VContainer;
using Object = UnityEngine.Object;

namespace Source.Infrastructure.MVVM.Factory
{
    public class ScreenFactory : IScreenFactory
    {
        private readonly Dictionary<Type, (string address, GameObject instance)> _screens = new();

        private readonly IObjectResolver _objectResolver;
        private readonly IResourceService _resourceService;

        public ScreenFactory(IObjectResolver objectResolver, IResourceService resourceService)
        {
            _objectResolver = objectResolver;
            _resourceService = resourceService;
        }

        public async UniTask<TView> CreateScreen<TView, TViewModel>(string address, Transform parent)
            where TView : ScreenView<TViewModel>
            where TViewModel : IScreenViewModel
        {
            Type viewType = typeof(TView);

            if (_screens.ContainsKey(viewType))
            {
                Debug.LogError("Screen already created: " + viewType);
                return _screens[viewType].instance.GetComponent<TView>();
            }

            GameObject prefab = await _resourceService.LoadAsset<GameObject>(address);
            GameObject instance = Object.Instantiate(prefab, parent);
            _screens[viewType] = (address, instance);

            TView view = instance.GetComponent<TView>();
            TViewModel viewModel = _objectResolver.Resolve<TViewModel>();
            viewModel.Initialize();
            view.Initialize(viewModel);
            return view;
        }

        public async UniTask<IScreenView> CreateScreen(Type viewType, Type viewModelType, string address, Transform parent)
        {
            if (_screens.ContainsKey(viewType))
            {
                Debug.LogError("Screen already created: " + viewType);
                return _screens[viewType].instance.GetComponent<IScreenView>();
            }

            GameObject prefab = await _resourceService.LoadAsset<GameObject>(address);
            GameObject instance = Object.Instantiate(prefab, parent);
            _screens[viewType] = (address, instance);

            var view = (IScreenView)instance.GetComponent(viewType);
            var viewModel = (IScreenViewModel)_objectResolver.Resolve(viewModelType);
            viewModel.Initialize();
            view.Initialize(viewModel);
            return view;
        }

        public async UniTask DisposeScreen<TView, TViewModel>()
            where TView : ScreenView<TViewModel>
            where TViewModel : IScreenViewModel
        {
            await DisposeScreen(typeof(TView), typeof(TViewModel));
        }

        public async UniTask DisposeScreen(Type viewType, Type viewModelType)
        {
            if (_screens.TryGetValue(viewType, out var entry))
            {
                var view = (IScreenView)entry.instance.GetComponent(viewType);
                var viewModel = (IScreenViewModel)_objectResolver.Resolve(viewModelType);
                viewModel.Dispose();
                view.Dispose();
                await view.Hide();
                Object.Destroy(entry.instance);

                _resourceService.ReleaseAsset(entry.address);
                _screens.Remove(viewType);
            }
        }
    }
}
