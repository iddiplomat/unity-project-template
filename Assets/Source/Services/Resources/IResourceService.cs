using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Source.Services.Resources
{
    public interface IResourceService
    {
        UniTask<T> LoadAsset<T>(string address) where T : Object;
        void ReleaseAsset(string address);
        void ReleaseAllAssets();
        bool HasLoadedAsset(string address);
    }
}
