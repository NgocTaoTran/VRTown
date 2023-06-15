using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using static GLTFast.GameObjectInstantiator;
using UnityEngine.SceneManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using VRTown.Model;

namespace VRTown.Service
{
    public interface IBundleLoader
    {
        void CheckPreloadAsset(object key, string errorMessage);

        void CheckPreloadAssets(List<string> keys, string errorMessage);

        void DownloadPreloadAsset(System.Action<DownloadConfig> callback, string key, string errorMessage);

        void DownloadPreloadAssets(System.Action<DownloadConfig> callback, List<string> keys, string errorMessage);

        UniTask<GameObject> InstantiateAssetAsync(string path);

        // LoadState GetLoadState(ScreenName moduleName);

        T LoadAssetLocal<T>(string path) where T : Object;

        UniTask<T> LoadAssetAsync<T>(string path) where T : class;

        UniTask<T> LoadAssetAsync<T>(AssetReference reference) where T : Object;

        void ReleaseAsset(string path);

        void ReleaseAsset(AsyncOperationHandle handle);

        void ReleaseAsset(AssetReference reference);

        void ReleaseInstance(GameObject instanceByAddressable);

        // UniTask<SceneInstance> LoadSceneAndActiveAsync(string sceneName, LoadSceneMode mode);

        // UniTask UnLoadScene(SceneInstance scene);
    }
}
