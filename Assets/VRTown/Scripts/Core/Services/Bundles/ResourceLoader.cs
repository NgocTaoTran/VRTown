using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using VRTown.Model;
using Zenject;

namespace VRTown.Service
{
    public class ResourceLoader : IBundleLoader
    {
        private readonly VRTown.Service.ILogger _logger;
        private readonly SignalBus _signalBus;

        public ResourceLoader(SignalBus signalBus, VRTown.Service.ILogger logger)
        {
            _signalBus = signalBus;
            _logger = logger;
        }

        public void CheckPreloadAsset(object key, string errorMessage)
        {
        }

        public void CheckPreloadAssets(List<string> keys, string errorMessage)
        {
        }

        public void DownloadPreloadAsset(System.Action<DownloadConfig> callback, string key, string errorMessage)
        {
        }

        public void DownloadPreloadAssets(System.Action<DownloadConfig> callback, List<string> keys, string errorMessage)
        {
        }

        public async UniTask<GameObject> InstantiateAssetAsync(string path)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                throw new ResourceRunOnEditorMode(path);
#endif
            try
            {
                GameObject pref = await LoadAssetAsync<GameObject>(path);
                GameObject result = Object.Instantiate(pref);
                return result;
            }
            catch
            {
                throw new MissinResourceAssetAtPath(path);
            }
        }

        public T LoadAssetLocal<T>(string path) where T : Object
        {
            try
            {
                var data = Resources.Load<T>(path);
                return (data as T);
            }
            catch
            {
                throw new MissinResourceAssetAtPath(path);
            }
        }

        // public LoadState GetLoadState(ScreenName moduleName)
        // {
        //     return LoadState.Loaded;
        // }

        public async UniTask<T> LoadAssetAsync<T>(string path) where T : class
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                throw new ResourceRunOnEditorMode(path);
#endif
            try
            {
                var request = await Resources.LoadAsync<Object>(path);
                return (request as T);
            }
            catch
            {
                throw new MissinResourceAssetAtPath(path);
            }
        }

        public UniTask<T> LoadAssetAsync<T>(AssetReference reference) where T : Object
        {
            throw new System.NotImplementedException();
        }

        public void ReleaseAsset(string path)
        {
            throw new System.NotImplementedException();
        }

        public void ReleaseAsset(AsyncOperationHandle handle)
        {
            throw new System.NotImplementedException();
        }

        public void ReleaseAsset(AssetReference reference)
        {
            throw new System.NotImplementedException();
        }

        public void ReleaseInstance(GameObject instanceByAddressable)
        {
            throw new System.NotImplementedException();
        }

        public UniTask<SceneInstance> LoadSceneAndActiveAsync(string sceneName, LoadSceneMode mode)
        {
            throw new System.NotImplementedException();
        }

        public UniTask UnLoadScene(SceneInstance scene)
        {
            throw new System.NotImplementedException();
        }

        private class MissinResourceAssetAtPath : System.Exception
        {
            public MissinResourceAssetAtPath(string message) : base(message) { }
        }

        private class ResourceRunOnEditorMode : System.Exception
        {
            public ResourceRunOnEditorMode(string message) : base(message) { }
        }
    }
}
