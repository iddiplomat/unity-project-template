using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using Source.Infrastructure.MVVM;
using Source.Infrastructure.MVVM.Factory;
using Source.Services.Resources;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer;

namespace Source.Infrastructure.MVVM.Tests
{
    public sealed class ScreenFactoryTests
    {
        private Transform _parent;

        [SetUp]
        public void SetUp()
        {
            _parent = new GameObject("ScreenFactoryTests_Parent").transform;
        }

        [TearDown]
        public void TearDown()
        {
            if (_parent != null)
                UnityEngine.Object.DestroyImmediate(_parent.gameObject);
        }

        [Test]
        public async Task CreateScreen_Generic_LoadsInstantiatesAndInitializes()
        {
            GameObject prefab = BuildPrefabWithView<FactoryTestScreenView>();
            var resourceService = new FakeResourceService(prefab);
            using IObjectResolver container = BuildContainer(b =>
            {
                b.Register<FactoryTestViewModel>(Lifetime.Singleton);
            });

            var factory = new ScreenFactory(container, resourceService);
            const string address = "test-address";

            FactoryTestScreenView view = await factory
                .CreateScreen<FactoryTestScreenView, FactoryTestViewModel>(address, _parent)
                .AsTask();

            Assert.That(resourceService.LoadedAddresses, Is.EqualTo(new[] { address }));
            Assert.That(view, Is.Not.Null);
            Assert.That(view.transform.parent, Is.SameAs(_parent));
            Assert.That(view.BoundViewModel, Is.Not.Null);
            Assert.That(view.BoundViewModel.InitializeCount, Is.EqualTo(1));
            Assert.That(view.BoundViewModel, Is.SameAs(container.Resolve<FactoryTestViewModel>()));
        }

        [Test]
        public async Task CreateScreen_ByType_LoadsInstantiatesAndInitializes()
        {
            GameObject prefab = BuildPrefabWithView<FactoryTestScreenView>();
            var resourceService = new FakeResourceService(prefab);
            using IObjectResolver container = BuildContainer(b =>
            {
                b.Register<FactoryTestViewModel>(Lifetime.Singleton);
            });

            var factory = new ScreenFactory(container, resourceService);
            const string address = "by-type-address";

            IScreenView view = await factory
                .CreateScreen(typeof(FactoryTestScreenView), typeof(FactoryTestViewModel), address, _parent)
                .AsTask();

            Assert.That(resourceService.LoadedAddresses, Is.EqualTo(new[] { address }));
            var concrete = (FactoryTestScreenView)view;
            Assert.That(concrete.BoundViewModel.InitializeCount, Is.EqualTo(1));
        }

        [Test]
        public async Task CreateScreen_Generic_Duplicate_LogsError_ReturnsExistingWithoutSecondLoadOrReInit()
        {
            LogAssert.Expect(LogType.Error, new Regex("Screen already created"));

            GameObject prefab = BuildPrefabWithView<FactoryTestScreenView>();
            var resourceService = new FakeResourceService(prefab);
            using IObjectResolver container = BuildContainer(b =>
            {
                b.Register<FactoryTestViewModel>(Lifetime.Singleton);
            });

            var factory = new ScreenFactory(container, resourceService);
            FactoryTestViewModel vm = container.Resolve<FactoryTestViewModel>();
            const string address = "dup-address";

            FactoryTestScreenView first = await factory
                .CreateScreen<FactoryTestScreenView, FactoryTestViewModel>(address, _parent)
                .AsTask();

            Assert.That(vm.InitializeCount, Is.EqualTo(1));
            Assert.That(_parent.childCount, Is.EqualTo(1));

            FactoryTestScreenView second = await factory
                .CreateScreen<FactoryTestScreenView, FactoryTestViewModel>(address, _parent)
                .AsTask();

            Assert.That(second, Is.SameAs(first));
            Assert.That(
                vm.InitializeCount,
                Is.EqualTo(1),
                "Duplicate CreateScreen must not call ViewModel.Initialize again.");
            Assert.That(resourceService.LoadedAddresses, Is.EqualTo(new[] { address }));
            Assert.That(
                _parent.childCount,
                Is.EqualTo(1),
                "Duplicate CreateScreen must not instantiate a second object under parent.");
        }

        [Test]
        public async Task CreateScreen_ByType_Duplicate_LogsError_ReturnsExistingWithoutSecondLoadOrReInit()
        {
            LogAssert.Expect(LogType.Error, new Regex("Screen already created"));

            GameObject prefab = BuildPrefabWithView<FactoryTestScreenView>();
            var resourceService = new FakeResourceService(prefab);
            using IObjectResolver container = BuildContainer(b =>
            {
                b.Register<FactoryTestViewModel>(Lifetime.Singleton);
            });

            var factory = new ScreenFactory(container, resourceService);
            FactoryTestViewModel vm = container.Resolve<FactoryTestViewModel>();
            const string address = "dup-by-type-address";

            IScreenView first = await factory
                .CreateScreen(typeof(FactoryTestScreenView), typeof(FactoryTestViewModel), address, _parent)
                .AsTask();

            Assert.That(vm.InitializeCount, Is.EqualTo(1));
            Assert.That(_parent.childCount, Is.EqualTo(1));

            IScreenView second = await factory
                .CreateScreen(typeof(FactoryTestScreenView), typeof(FactoryTestViewModel), address, _parent)
                .AsTask();

            Assert.That(second, Is.SameAs(first));
            Assert.That(vm.InitializeCount, Is.EqualTo(1));
            Assert.That(resourceService.LoadedAddresses, Is.EqualTo(new[] { address }));
            Assert.That(_parent.childCount, Is.EqualTo(1));
        }

        [Test]
        public async Task DisposeScreen_ReleasesAssetAndDisposesViewModel()
        {
            GameObject prefab = BuildPrefabWithView<FactoryTestScreenView>();
            var resourceService = new FakeResourceService(prefab);
            using IObjectResolver container = BuildContainer(b =>
            {
                b.Register<FactoryTestViewModel>(Lifetime.Singleton);
            });

            var factory = new ScreenFactory(container, resourceService);
            const string address = "dispose-address";

            await factory.CreateScreen<FactoryTestScreenView, FactoryTestViewModel>(address, _parent).AsTask();
            FactoryTestViewModel vm = container.Resolve<FactoryTestViewModel>();

            await factory.DisposeScreen<FactoryTestScreenView, FactoryTestViewModel>().AsTask();

            Assert.That(vm.DisposeCount, Is.EqualTo(1));
            Assert.That(resourceService.ReleasedAddresses, Is.EqualTo(new[] { address }));
        }

        [Test]
        public async Task DisposeScreen_WhenNotCreated_DoesNotThrowOrRelease()
        {
            var resourceService = new FakeResourceService(BuildPrefabWithView<FactoryTestScreenView>());
            using IObjectResolver container = BuildContainer(b =>
            {
                b.Register<FactoryTestViewModel>(Lifetime.Singleton);
            });

            var factory = new ScreenFactory(container, resourceService);

            await factory.DisposeScreen<FactoryTestScreenView, FactoryTestViewModel>().AsTask();

            Assert.That(resourceService.ReleasedAddresses, Is.Empty);
        }

        private static GameObject BuildPrefabWithView<TView>() where TView : Component
        {
            var prefab = new GameObject("Prefab");
            prefab.AddComponent<CanvasGroup>();
            prefab.AddComponent<TView>();
            return prefab;
        }

        private static IObjectResolver BuildContainer(Action<ContainerBuilder> configure)
        {
            var builder = new ContainerBuilder();
            configure(builder);
            return builder.Build();
        }

        private sealed class FakeResourceService : IResourceService
        {
            private readonly GameObject _prefab;

            public FakeResourceService(GameObject prefab) => _prefab = prefab;

            public List<string> LoadedAddresses { get; } = new();
            public List<string> ReleasedAddresses { get; } = new();

            public UniTask<T> LoadAsset<T>(string address) where T : UnityEngine.Object
            {
                LoadedAddresses.Add(address);
                return UniTask.FromResult((T)(UnityEngine.Object)_prefab);
            }

            public void ReleaseAsset(string address) => ReleasedAddresses.Add(address);

            public void ReleaseAllAssets()
            {
            }

            public bool HasLoadedAsset(string address) => LoadedAddresses.Contains(address);
        }

        private sealed class FactoryTestViewModel : IScreenViewModel
        {
            public int InitializeCount { get; private set; }
            public int DisposeCount { get; private set; }

            public void Initialize() => InitializeCount++;

            public void Dispose() => DisposeCount++;
        }

        private sealed class FactoryTestScreenView : ScreenView<FactoryTestViewModel>
        {
            public FactoryTestViewModel BoundViewModel { get; private set; }

            protected override void Awake()
            {
                canvasGroup = GetComponent<CanvasGroup>();
                base.Awake();
            }

            protected override void OnBind() => BoundViewModel = ViewModel;
        }
    }
}
