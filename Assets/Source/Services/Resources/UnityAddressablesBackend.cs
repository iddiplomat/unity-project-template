using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Source.Services.Resources
{
    public sealed class UnityAddressablesBackend : IAddressablesBackend
    {
        public AsyncOperationHandle<T> LoadAssetAsync<T>(string address) where T : Object =>
            Addressables.LoadAssetAsync<T>(address);

        public void Release(AsyncOperationHandle handle) =>
            Addressables.Release(handle);
    }
}
