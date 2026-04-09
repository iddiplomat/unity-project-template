using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Source.Services.Resources
{
    internal static class AddressablesHandleUtility
    {
        private static readonly object InitGate = new();
        private static bool _resourceManagerReady;

        public static AsyncOperationHandle<T> CreateCompleted<T>(T result) where T : Object
        {
            EnsureResourceManagerReady();
            return Addressables.ResourceManager.CreateCompletedOperation(result, string.Empty);
        }

        private static void EnsureResourceManagerReady()
        {
            if (_resourceManagerReady)
                return;

            lock (InitGate)
            {
                if (_resourceManagerReady)
                    return;

                if (Addressables.ResourceManager == null)
                {
                    AsyncOperationHandle init = Addressables.InitializeAsync();
                    init.WaitForCompletion();
                }

                _resourceManagerReady = true;
            }
        }
    }
}
