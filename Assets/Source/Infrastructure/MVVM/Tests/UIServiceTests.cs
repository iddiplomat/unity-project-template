using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using Source.Infrastructure.MVVM;
using Source.Infrastructure.MVVM.Factory;
using Source.Infrastructure.MVVM.UI;
using UnityEngine;

namespace Source.Infrastructure.MVVM.Tests
{
    public sealed class UIServiceTests
    {
        [Test]
        public async Task ShowScreen_Generic_PassesAddressFromScreenType()
        {
            var factory = new RecordingScreenFactory();
            var service = new UIService(factory, new StubScreenRegistry());
            var parent = new GameObject("UIServiceTests_Parent").transform;

            try
            {
                IScreenView view = await service
                    .ShowScreen<UiTestScreenView, UiTestViewModel>(UIScreenType.MainMenu, parent)
                    .AsTask();

                Assert.That(view, Is.Null);
                Assert.That(factory.LastGenericAddress, Is.EqualTo(UIScreenType.MainMenu.GetAddress()));
                Assert.That(factory.LastGenericParent, Is.SameAs(parent));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(parent.gameObject);
            }
        }

        [Test]
        public async Task ShowScreen_FromRegistry_PassesBindingTypesAndAddress()
        {
            var factory = new RecordingScreenFactory();
            var binding = new ScreenBinding(typeof(UiTestScreenView), typeof(UiTestViewModel));
            var registry = new StubScreenRegistry { Binding = binding };
            var service = new UIService(factory, registry);
            var parent = new GameObject("UIServiceTests_Parent2").transform;

            try
            {
                IScreenView view = await service.ShowScreen(UIScreenType.CreateSessionPopUp, parent).AsTask();

                Assert.That(view, Is.Null);
                Assert.That(registry.LastRequestedType, Is.EqualTo(UIScreenType.CreateSessionPopUp));
                Assert.That(factory.LastViewType, Is.EqualTo(typeof(UiTestScreenView)));
                Assert.That(factory.LastViewModelType, Is.EqualTo(typeof(UiTestViewModel)));
                Assert.That(factory.LastNonGenericAddress, Is.EqualTo(UIScreenType.CreateSessionPopUp.GetAddress()));
                Assert.That(factory.LastNonGenericParent, Is.SameAs(parent));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(parent.gameObject);
            }
        }

        [Test]
        public async Task HideScreen_Generic_CallsFactoryDispose()
        {
            var factory = new RecordingScreenFactory();
            var service = new UIService(factory, new StubScreenRegistry());

            await service.HideScreen<UiTestScreenView, UiTestViewModel>().AsTask();

            Assert.That(factory.GenericDisposeCalled, Is.True);
        }

        [Test]
        public async Task HideScreen_FromRegistry_CallsFactoryDisposeWithBindingTypes()
        {
            var factory = new RecordingScreenFactory();
            var binding = new ScreenBinding(typeof(UiTestScreenView), typeof(UiTestViewModel));
            var registry = new StubScreenRegistry { Binding = binding };
            var service = new UIService(factory, registry);

            await service.HideScreen(UIScreenType.MainMenu).AsTask();

            Assert.That(registry.LastRequestedType, Is.EqualTo(UIScreenType.MainMenu));
            Assert.That(factory.LastDisposeViewType, Is.EqualTo(typeof(UiTestScreenView)));
            Assert.That(factory.LastDisposeViewModelType, Is.EqualTo(typeof(UiTestViewModel)));
        }

        private sealed class RecordingScreenFactory : IScreenFactory
        {
            public string LastGenericAddress { get; private set; }
            public Transform LastGenericParent { get; private set; }
            public string LastNonGenericAddress { get; private set; }
            public Transform LastNonGenericParent { get; private set; }
            public Type LastViewType { get; private set; }
            public Type LastViewModelType { get; private set; }
            public Type LastDisposeViewType { get; private set; }
            public Type LastDisposeViewModelType { get; private set; }
            public bool GenericDisposeCalled { get; private set; }

            public UniTask<TView> CreateScreen<TView, TViewModel>(string address, Transform parent)
                where TView : ScreenView<TViewModel>
                where TViewModel : IScreenViewModel
            {
                LastGenericAddress = address;
                LastGenericParent = parent;
                return UniTask.FromResult<TView>(null);
            }

            public UniTask<IScreenView> CreateScreen(Type viewType, Type viewModelType, string address, Transform parent)
            {
                LastViewType = viewType;
                LastViewModelType = viewModelType;
                LastNonGenericAddress = address;
                LastNonGenericParent = parent;
                return UniTask.FromResult<IScreenView>(null);
            }

            public UniTask DisposeScreen<TView, TViewModel>()
                where TView : ScreenView<TViewModel>
                where TViewModel : IScreenViewModel
            {
                GenericDisposeCalled = true;
                return UniTask.CompletedTask;
            }

            public UniTask DisposeScreen(Type viewType, Type viewModelType)
            {
                LastDisposeViewType = viewType;
                LastDisposeViewModelType = viewModelType;
                return UniTask.CompletedTask;
            }
        }

        private sealed class StubScreenRegistry : IScreenRegistry
        {
            public ScreenBinding Binding { get; set; } =
                new(typeof(UiTestScreenView), typeof(UiTestViewModel));

            public UIScreenType LastRequestedType { get; private set; }

            public ScreenBinding GetBinding(UIScreenType screenType)
            {
                LastRequestedType = screenType;
                return Binding;
            }
        }

        private sealed class UiTestViewModel : IScreenViewModel
        {
            public void Initialize()
            {
            }

            public void Dispose()
            {
            }
        }

        private sealed class UiTestScreenView : ScreenView<UiTestViewModel>
        {
            protected override void OnBind()
            {
            }
        }
    }
}
