using System.Collections;
using System.Collections.Generic;
using System.IO;
using GLTFast;
using Newtonsoft.Json;
using UnityEngine;
using VRTown.Model;
using VRTown.Provider;

namespace VRTown.Game
{
    public class PlotLoader : MonoBehaviour
    {
        private List<LoadModelTask> queueLoadingTasks;
        private List<LoadModelTask> loadingTasks;
        private Dictionary<string, LoadModelTask> taskByAssetName;

        // DownloadProvider _downloader = new DownloadProvider();

        // public async void LoadSceneZip(LandEntity landEntity, MonoBehaviour obj, string path, Vector3 position, System.Action<GameObject> onSuccess)
        // {
        //     queueLoadingTasks = new List<LoadModelTask>();
        //     loadingTasks = new List<LoadModelTask>();
        //     taskByAssetName = new Dictionary<string, LoadModelTask>();

        //     var urlTransformAll = new UrlTransformAll();
        //     var transformPath = urlTransformAll.Transform(path);

        //     var downloadTask = _downloader.Request(new System.Uri($"{transformPath}/builder.json"));
        //     await downloadTask;

        //     var downloadResult = downloadTask.Result;
        //     if (!downloadResult.success)
        //     {
        //         Debug.Log(downloadResult.error);
        //     }
        //     else
        //     {
        //         // Show results as text
        //         var _data = downloadResult.data;
        //         if (_data == null) yield break;
        //         using var stream = new MemoryStream(_data);
        //         using var sceneStream = new StreamReader(new MemoryStream(_data));

        //         using var jsonReader = new JsonTextReader(new StreamReader(new MemoryStream(_data)));
        //         JsonSerializer serializer = new JsonSerializer();
        //         var landData = serializer.Deserialize<Land>(jsonReader);
        //         var sceneData = landData.scene;

        //         var sceneRoot = new GameObject(sceneData.id);
        //         if (landEntity != null) landEntity.gameObjects.Add(sceneRoot);
        //         var assetLoaderId = 1;
        //         // build the scene
        //         foreach (var entityGroupData in sceneData.entities)
        //         {
        //             var entityData = entityGroupData.Value;
        //             var entity = new GameObject(entityData.name);
        //             entity.transform.SetParent(sceneRoot.transform);
        //             foreach (var componentId in entityData.components)
        //             {
        //                 var componentData = sceneData.components[componentId];
        //                 if (componentData != null)
        //                 {
        //                     if (componentData.type == "Transform")
        //                     {
        //                         var trans = entity.transform;
        //                         trans.localPosition = componentData.data.position + position;
        //                         trans.localRotation = componentData.data.rotation;
        //                         trans.localScale = componentData.data.scale;
        //                     }
        //                     else if (componentData.type == "GLTFShape" || componentData.type == "Script")
        //                     {
        //                         var node = new BuilderNode()
        //                         {
        //                             transform = entity.transform,
        //                             values = componentData.data.values,
        //                         };
        //                         var assetData = sceneData.assets[componentData.data.assetId];
        //                         if (assetData != null)
        //                         {
        //                             if (taskByAssetName.ContainsKey(componentData.data.assetId))
        //                             {
        //                                 var task = taskByAssetName[componentData.data.assetId];
        //                                 task.objectParent.Add(node);
        //                             }
        //                             else
        //                             {
        //                                 var contents = assetData.contents;
        //                                 var fullPaths = new List<string>();
        //                                 foreach (var contentData in contents)
        //                                 {
        //                                     if (contentData.Key.EndsWith(".gltf") || contentData.Key.EndsWith(".glb") || contentData.Key.EndsWith(".lua"))
        //                                     {
        //                                         var fullPath = $"{transformPath}/{assetData.id}/{contentData.Key}";
        //                                         fullPaths.Add(fullPath);
        //                                     }
        //                                 }

        //                                 if (fullPaths.Count > 0)
        //                                 {
        //                                     // load
        //                                     queueLoadingTasks.Add(taskByAssetName[componentData.data.assetId] = new LoadModelTask
        //                                     {
        //                                         objectParent = new List<BuilderNode>() { node },
        //                                         fullPaths = fullPaths,
        //                                         loadId = assetLoaderId++,
        //                                     });
        //                                 }
        //                             }
        //                         }
        //                     }
        //                 }
        //             }
        //         }

        //         // load mod≈≈≈≈xe
        //         await LoadModels(async () =>
        //         {
        //             string script = await GetSceneScriptInZip(transformPath);
        //             //Debug.LogErrorFormat("===path {0}, {1}", transformPath, script);
        //             if (script != null)
        //             {
        //                 obj.StartCoroutine(RunOnMainThread(() =>
        //                 {
        //                     try
        //                     {
        //                         LuaInterface.RunLuaScript(sceneRoot, script);
        //                     }
        //                     catch (Exception e)
        //                     {
        //                         Debug.LogErrorFormat("[lua][error] {0}", e.ToString());
        //                         Debug.LogError(e);
        //                     }
        //                     if (onSuccess != null) onSuccess(sceneRoot);
        //                 }));
        //             }
        //         }));
        //     };
        // }

        // async void LoadModels(System.Action onSuccess)
        // {
        //     const int maxTasksAtOnce = 3;
        //     while (true)
        //     {
        //         if (loadingTasks.Count >= maxTasksAtOnce)
        //         {
        //             yield return null;
        //         }
        //         if (queueLoadingTasks.Count <= 0)
        //         {
        //             if (onSuccess != null) onSuccess();
        //             yield break;
        //         }
        //         while (loadingTasks.Count < maxTasksAtOnce && queueLoadingTasks.Count > 0)
        //         {
        //             var taskIdx = queueLoadingTasks.Count - 1;
        //             var task = queueLoadingTasks[taskIdx];
        //             queueLoadingTasks.RemoveAt(taskIdx);
        //             if (task.objectParent[0].transform.activeSelf())
        //             {
        //                 LoadModel(obj, task);
        //             }
        //             yield return null;
        //         }

        //         yield return null;
        //     }
        // }

        // async void LoadModel(MonoBehaviour obj, LoadModelTask task)
        // {
        //     loadingTasks.Add(task);
        //     var gltf = new GLTFast.GltfImport(logger: new ConsoleLogger(), downloadProvider: _downloader);

        //     // Create a settings object and configure it accordingly
        //     var settings = new ImportSettings
        //     {
        //         generateMipMaps = false,
        //         anisotropicFilterLevel = 3,
        //         nodeNameMethod = ImportSettings.NameImportMethod.OriginalUnique
        //     };

        //     //Debug.Log($"load fullPath: ${task.fullPaths}");

        //     var success = true;
        //     string script = null;
        //     foreach (var fullPath in task.fullPaths)
        //     {
        //         if (fullPath.EndsWith(".glb") || fullPath.EndsWith(".gltf"))
        //         {
        //             try
        //             {
        //                 var _success = fullPath.StartsWith("http")
        //                     ? await gltf.Load(new System.Uri(fullPath), settings)
        //                     : await gltf.Load(fullPath, settings);
        //                 if (!_success)
        //                 {
        //                     success = false;
        //                     break;
        //                 }
        //             }
        //             catch (System.Exception e)
        //             {
        //                 success = false;
        //                 break;
        //             }
        //         }
        //         else if (fullPath.EndsWith(".lua"))
        //         {
        //             Debug.LogErrorFormat("Lua file: {0}", fullPath);
        //             var zipDownload = new ZipDownloadProvider();
        //             var download = await zipDownload.Request(new Uri(fullPath));
        //             script = download.text;
        //             Debug.LogErrorFormat("Lua file: script {0}", script);
        //         }
        //     }

        //     if (success)
        //     {
        //         foreach (var node in task.objectParent)
        //         {
        //             var transform = node.transform;
        //             try
        //             {
        //                 if (transform != null && transform.activeSelf())
        //                 {
        //                     if (gltf.InstantiateScene(transform))
        //                     {
        //                         ProcessCollider(transform);
        //                         if (script != null)
        //                         {
        //                             Debug.LogErrorFormat("parent script: {0}", transform.gameObject.name);
        //                             obj.StartCoroutine(RunOnMainThread(() =>
        //                             {
        //                                 LuaInterface.RunLuaScript(transform.gameObject, script, node.values);
        //                             }));
        //                         }
        //                     }
        //                 }
        //             }
        //             catch (MissingReferenceException)
        //             {
        //             }
        //         }
        //         OnLoad(task);
        //     }
        //     else
        //     {
        //         Debug.LogError("Loading glTF failed!");
        //         OnLoad(task);
        //     }
        // }
    }
}