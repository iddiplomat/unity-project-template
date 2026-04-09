using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Source.Services.Resources
{
    internal static class ResourceServiceTaskExtensions
    {
        public static Task<T> LoadAssetAsTask<T>(this ResourceService service, string address)
            where T : Object =>
            service.LoadAsset<T>(address).AsTask();
    }
}
