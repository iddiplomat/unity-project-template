using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace Source.Services.Resources
{
    public class ResourceService : IResourceService
    {
        private readonly Dictionary<string, AsyncOperationHandle> _handles = new();

        public async UniTask<T> LoadAsset<T>(string address) where T : Object
        {
            if (_handles.TryGetValue(address, out var existingHandle))
            {
                return existingHandle.Result as T;
            }

            var handle = Addressables.LoadAssetAsync<T>(address);
            _handles[address] = handle;

            T result = await handle;
            return result;
        }

        public void ReleaseAsset(string address)
        {
            if (!_handles.TryGetValue(address, out var handle))
                return;

            Addressables.Release(handle);
            _handles.Remove(address);
        }

        public void ReleaseAllAssets()
        {
            foreach (var handle in _handles.Values)
                Addressables.Release(handle);

            _handles.Clear();
        }

        public bool HasLoadedAsset(string address) =>
            _handles.ContainsKey(address);
    }
}
