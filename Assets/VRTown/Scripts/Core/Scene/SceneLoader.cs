using UnityEngine;
#if !UNITY_5_2
using UnityEngine.SceneManagement;
#endif

namespace VRTown.Scene
{
    public interface SceneLoader
    {
        void PreLoad();
        void Load();
        bool IsFinished();
        void PreLoadDone();
        void ActiveScene();
    }

    public class SceneFileLoader : SceneLoader
    {
        public string fileName;

        private AsyncOperation _loadOperation;

        public void PreLoad()
        {
            _loadOperation = SceneManager.LoadSceneAsync(fileName, LoadSceneMode.Additive);
            _loadOperation.allowSceneActivation = false;
        }

        public void PreLoadDone()
        {
            _loadOperation.allowSceneActivation = true;
        }

        public void Load()
        {

        }

        public bool IsFinished()
        {
            return _loadOperation.isDone;
        }

        public void ActiveScene()
        {

        }
    }

    public class UnloadSceneFileLoader : SceneLoader
    {
        public string fileName;

        private AsyncOperation _loadOperation;

        public void PreLoad()
        {

        }

        public void PreLoadDone()
        {

        }

        public void Load()
        {
            _loadOperation = SceneManager.UnloadSceneAsync(fileName);
        }

        public bool IsFinished()
        {
            return _loadOperation.isDone;
        }

        public void ActiveScene()
        {
            Resources.UnloadUnusedAssets();
        }
    }

    public class ScenePrefabLoader : SceneLoader
    {
        public string prefabPath;

        private ResourceRequest _loadRequest;

        public void PreLoad()
        {
            _loadRequest = Resources.LoadAsync(prefabPath);
            _loadRequest.allowSceneActivation = false;
        }

        public void PreLoadDone()
        {
            _loadRequest.allowSceneActivation = true;

            var scene = SceneManager.GetActiveScene();
            var activeObjects = scene.GetRootGameObjects();
            foreach (GameObject obj in activeObjects)
            {
                GameObject.Destroy(obj);
            }

            var sceneObj = GameObject.Instantiate(_loadRequest.asset) as GameObject;
            sceneObj.name = "SceneRoot";
        }

        public void Load()
        {

        }

        public bool IsFinished()
        {
            return _loadRequest.isDone;
        }

        public void ActiveScene()
        {

        }
    }

    public class SceneDefaultEmptyLoader<T> : SceneLoader where T : MonoBehaviour
    {
        private bool _isLoaded;
        private GameObject _sceneObj;

        public void PreLoad()
        {
            _sceneObj = new GameObject("EmptySceneRoot");
            _sceneObj.AddComponent<T>();
            _sceneObj.SetActive(false);
            _isLoaded = true;
        }

        public void PreLoadDone()
        {
            var scene = SceneManager.GetActiveScene();
            var activeObjects = scene.GetRootGameObjects();
            foreach (GameObject obj in activeObjects)
            {
                GameObject.Destroy(obj);
            }

            _sceneObj.SetActive(true);
        }

        public void Load()
        {

        }

        public bool IsFinished()
        {
            return _isLoaded;
        }

        public void ActiveScene()
        {

        }
    }
}