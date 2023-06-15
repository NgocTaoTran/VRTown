#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using VRTown.Scene;

namespace VRTown.Editor
{
    public static class UtilMenu
    {
        [MenuItem("VRTown/Utils/Data/Clear Player Pref", false, 20)]
        public static void ClearPlayerPref()
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("Cleared player preferences!");
        }

        [MenuItem("VRTown/Utils/Data/Clear Persistant Data", false, 20)]
        public static void ClearPersistantData()
        {
            Debug.Log("Persistant data located at :" + Application.persistentDataPath);
            DirectoryInfo dir = new DirectoryInfo(Application.persistentDataPath);
            dir.Delete(true);
            Debug.Log("Deleted player persistant data!");
        }

        [MenuItem("VRTown/Utils/Data/Clear temporary Data", false, 20)]
        public static void ClearTempData()
        {
            Debug.Log("Temporary data located at :" + Application.temporaryCachePath);
            DirectoryInfo dir = new DirectoryInfo(Application.temporaryCachePath);
            dir.Delete(true);
            Debug.Log("Deleted player temporary data!");
        }

        [MenuItem("VRTown/Utils/Data/Clear All", false, 12)]
        public static void ClearAll()
        {
            ClearPlayerPref();
            ClearPersistantData();
            ClearTempData();
        }

        [MenuItem("VRTown/Utils/Memory/UnloadAllUnusedAssets", false, 20)]
        public static void UnloadAllUnusedAsset()
        {
            Debug.Log("Unload all unused assets ");

            Resources.UnloadUnusedAssets();
            EditorUtility.UnloadUnusedAssetsImmediate();
        }

        [MenuItem("VRTown/Utils/Setup Boostrap Scene", false, 30)]
        public static void SetupBootstrapScene()
        {
            // clean up current scene
            var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var rootObjects = activeScene.GetRootGameObjects();
            foreach (var obj in rootObjects)
                GameObject.DestroyImmediate(obj);

            // create SceneDirector
            var sceneDirector = new GameObject("SceneDirector", typeof(SceneDirector)).GetComponent<SceneDirector>();
            var transitionCamera = new GameObject("TransitionCamera", typeof(Camera)).GetComponent<Camera>();
            sceneDirector.OverlayCamera = transitionCamera;
            transitionCamera.transform.SetParent(sceneDirector.transform, false);
            transitionCamera.clearFlags = CameraClearFlags.Depth;
            transitionCamera.orthographic = false;
            transitionCamera.fieldOfView = 60;
            transitionCamera.nearClipPlane = 0.01f;
            transitionCamera.farClipPlane = 100f;
            transitionCamera.useOcclusionCulling = false;
            transitionCamera.allowHDR = transitionCamera.allowMSAA = false;

            LayerMaskEx.CreateLayer("ScreenOverlay");
            var transitionLayer = LayerMask.NameToLayer("ScreenOverlay");
            transitionCamera.cullingMask = 1 << transitionLayer;
            transitionCamera.gameObject.SetActive(false);

            // create UIManager
            var UIManager = new GameObject("UIManager", typeof(UIManager)).GetComponent<UIManager>();
            UIManager.BaseUIPath = "UIPrefabs";
            UIManager.UnlitTextureColorMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/seeker/Materials/unlit-texturecolor.mat");

            // UI camera
            var UICamera = new GameObject("UICamera", typeof(Camera)).GetComponent<Camera>();
            UIManager.CameraUI = UICamera;
            UICamera.transform.SetParent(UIManager.transform, false);
            UICamera.clearFlags = CameraClearFlags.Depth;
            UICamera.orthographic = true;
            UICamera.orthographicSize = 6.4f;
            UICamera.nearClipPlane = -100f;
            UICamera.farClipPlane = 100f;
            UICamera.useOcclusionCulling = false;
            UICamera.allowHDR = UICamera.allowMSAA = false;

            LayerMaskEx.CreateLayer("UI");
            var uiLayer = LayerMask.NameToLayer("UI");
            UICamera.cullingMask = 1 << uiLayer;

            // create EventSystem
            var eventSystem = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem)).GetComponent<UnityEngine.EventSystems.EventSystem>();
            UIManager.EventSystem = eventSystem;
            eventSystem.transform.SetParent(UIManager.transform, false);
            eventSystem.gameObject.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            // create MainCanvas
            var mainCanvas = new GameObject("MainCanvas", typeof(Canvas)).GetComponent<Canvas>();
            mainCanvas.transform.SetParent(UIManager.transform, false);
            mainCanvas.tag = "MainCanvas";
            mainCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            mainCanvas.pixelPerfect = false;
            mainCanvas.worldCamera = UICamera;
            mainCanvas.planeDistance = 0f;
            mainCanvas.gameObject.layer = uiLayer;

            var canvasRaycast = mainCanvas.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            canvasRaycast.ignoreReversedGraphics = true;
            canvasRaycast.blockingObjects = UnityEngine.UI.GraphicRaycaster.BlockingObjects.None;

            // portrait scaler
            var canvasScaler = mainCanvas.gameObject.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasScaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(720f, 1280f);
            canvasScaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 1f;
            canvasScaler.referencePixelsPerUnit = 100f;
            mainCanvas.gameObject.AddComponent<CanvasScalerPortrait>();

            // game rect
            var gameRect = new GameObject("GameRect", typeof(RectTransform)).GetComponent<RectTransform>();
            UIManager.GameRect = gameRect;
            gameRect.SetParent(mainCanvas.transform, false);
            gameRect.anchorMin = Vector2.zero;
            gameRect.anchorMax = Vector2.one;
            gameRect.offsetMin = gameRect.offsetMax = Vector2.zero;
            gameRect.gameObject.layer = uiLayer;

            // default UI layer rect
            var uiLayerRect = new GameObject("LayerUI", typeof(RectTransform)).GetComponent<RectTransform>();
            UIManager.UILayers = new List<Transform>() { uiLayerRect };
            uiLayerRect.SetParent(gameRect, false);
            uiLayerRect.anchorMin = Vector2.zero;
            uiLayerRect.anchorMax = Vector2.one;
            uiLayerRect.offsetMin = uiLayerRect.offsetMax = Vector2.zero;
            uiLayerRect.gameObject.layer = uiLayer;
        }
    }
}
#endif