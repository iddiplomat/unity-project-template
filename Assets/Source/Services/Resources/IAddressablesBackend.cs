using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Source.Services.Resources
{
    public interface IAddressablesBackend
    {
        AsyncOperationHandle<T> LoadAssetAsync<T>(string address) where T : Object;

        void Release(AsyncOperationHandle handle);
    }
}
