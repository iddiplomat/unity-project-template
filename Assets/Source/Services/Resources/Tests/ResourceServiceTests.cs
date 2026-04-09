using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Source.Services.Resources;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace Source.Services.Resources.Tests
{
    public sealed class ResourceServiceTests
    {
        private FakeAddressablesBackend _backend;
        private ResourceService _service;
        private Texture2D _texture;

        [SetUp]
        public void SetUp()
        {
            _backend = new FakeAddressablesBackend();
            _service = new ResourceService(_backend);
            _texture = new Texture2D(4, 4);
        }

        [TearDown]
        public void TearDown()
        {
            _service.ReleaseAllAssets();
            if (_texture != null)
                Object.DestroyImmediate(_texture);
        }

        [Test]
        public async Task LoadAsset_LoadsViaBackend_ReturnsAssetAndTracksHandle()
        {
            const string address = "test-address-load";
            _backend.RegisterAsset(address, _texture);

            Texture2D loaded = await _service.LoadAssetAsTask<Texture2D>(address);

            Assert.That(loaded, Is.SameAs(_texture));
            Assert.That(_service.HasLoadedAsset(address), Is.True);
            Assert.That(_backend.LoadAsyncInvocationCount, Is.EqualTo(1));
        }

        [Test]
        public async Task LoadAsset_SameAddressTwice_DoesNotCallBackendAgain()
        {
            const string address = "test-address-cache";
            _backend.RegisterAsset(address, _texture);

            await _service.LoadAssetAsTask<Texture2D>(address);
            Texture2D second = await _service.LoadAssetAsTask<Texture2D>(address);

            Assert.That(second, Is.SameAs(_texture));
            Assert.That(_backend.LoadAsyncInvocationCount, Is.EqualTo(1));
        }

        [Test]
        public void ReleaseAsset_WhenNotLoaded_DoesNotCallBackendRelease()
        {
            _service.ReleaseAsset("missing-address");
            Assert.That(_backend.ReleaseInvocationCount, Is.EqualTo(0));
        }

        [Test]
        public async Task ReleaseAsset_AfterLoad_CallsBackendAndRemovesFromService()
        {
            const string address = "test-address-release";
            _backend.RegisterAsset(address, _texture);
            await _service.LoadAssetAsTask<Texture2D>(address);

            _service.ReleaseAsset(address);

            Assert.That(_service.HasLoadedAsset(address), Is.False);
            Assert.That(_backend.ReleaseInvocationCount, Is.EqualTo(1));
        }

        [Test]
        public async Task ReleaseAllAssets_ReleasesEveryLoadedHandle()
        {
            var other = new Texture2D(2, 2);
            try
            {
                _backend.RegisterAsset("addr-a", _texture);
                _backend.RegisterAsset("addr-b", other);
                await _service.LoadAssetAsTask<Texture2D>("addr-a");
                await _service.LoadAssetAsTask<Texture2D>("addr-b");

                _service.ReleaseAllAssets();

                Assert.That(_service.HasLoadedAsset("addr-a"), Is.False);
                Assert.That(_service.HasLoadedAsset("addr-b"), Is.False);
                Assert.That(_backend.ReleaseInvocationCount, Is.EqualTo(2));
            }
            finally
            {
                Object.DestroyImmediate(other);
            }
        }

        private sealed class FakeAddressablesBackend : IAddressablesBackend
        {
            private readonly Dictionary<string, Object> _registry = new();

            public int LoadAsyncInvocationCount { get; private set; }
            public int ReleaseInvocationCount { get; private set; }

            public void RegisterAsset<T>(string address, T asset) where T : Object =>
                _registry[address] = asset;

            public AsyncOperationHandle<T> LoadAssetAsync<T>(string address) where T : Object
            {
                LoadAsyncInvocationCount++;
                if (!_registry.TryGetValue(address, out Object obj))
                    throw new KeyNotFoundException(address);

                return AddressablesHandleUtility.CreateCompleted((T)obj);
            }

            public void Release(AsyncOperationHandle handle)
            {
                ReleaseInvocationCount++;
            }
        }
    }
}
