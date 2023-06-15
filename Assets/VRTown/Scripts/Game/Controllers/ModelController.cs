using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRTown.Network;
using Zenject;
using VRTown.Model;
using VRTown.Utilities;
using VRTown.Service;
using Cysharp.Threading.Tasks;
using GLTFast;
using VRTown.Provider;
using System.Threading.Tasks;
using VRTown.Game.LuaSystem;
using GLTFast.Logging;
using System.IO.Compression;
using System.IO;
using Newtonsoft.Json;

namespace VRTown.Game
{
    public interface IModelController
    {
        UniTask Initialize();
        GModel CreateCharacter(UserData userData);
        void RemoveCharacter(GModel model);
        Task<bool> LoadSingleModel(GameObject root, string fullPath);
        Task<Texture> LoadSingleTexture(string path);

        // Lands
        UniTask<LandEntity> CreateLand(LandData landData);
        UniTask<LocalLand> LoadLocalModel(ModelLocal model);
        UniTask StoreLand(List<SubLandEntity> entities);
        void ChangeSkin(ref GModel model, UserData newData);
        void CreateProp(GModel model, PropData data, int colorId = -1);
        void ChangeSkinColor(GModel model, int newColor = -1);
        void UpdateEquipments(GModel model, UserData newData); // Sau khi sua ham xong neu chay test okir thi remove cai cu di, mac dinh la okie
        void ClearProps(GModel model);
    }

    public partial class ModelController : IModelController
    {
        #region Zenject_Binding
        [Inject] VRTown.Service.ILogger _logger;
        [Inject(Id = BundleLoaderName.Addressable)] readonly IBundleLoader _remoteLoader;
        [Inject(Id = BundleLoaderName.Zip)] readonly IBundleLoader _zipLoader;
        [Inject] readonly ConfigController _configController;
        [Inject] readonly ModelConfig _modelConfig;
        [Inject] readonly LuaInterface _luaInterface;
        [Inject] readonly LuaScript.Factory _luaFactory;
        #endregion Zenject_Binding

        #region Local
        string _baseUrl = "";
        DownloadProvider _downloadProvider;
        Dictionary<string, Transform> _mapModels = new Dictionary<string, Transform>();
        #endregion Local

        #region Object_Pool
        Transform _tfPoolParent = null;
        Dictionary<string, ObjectPool<Transform>> _dictCharacters = new Dictionary<string, ObjectPool<Transform>>();
        Dictionary<string, PartMesh> _cacheParts = new Dictionary<string, PartMesh>();
        Dictionary<string, Task> _partLoadings = new Dictionary<string, Task>();
        Dictionary<string, ObjectPool<Transform>> _poolCharacterProps = new Dictionary<string, ObjectPool<Transform>>();
        Dictionary<string, RequestRemoteAsset> _cacheLandAssets = new Dictionary<string, RequestRemoteAsset>();
        Dictionary<string, ObjectPool<Transform>> _poolLandModels = new Dictionary<string, ObjectPool<Transform>>();
        #endregion Object_Pool

        public async UniTask Initialize()
        {
            _logger.Log("[ModelController] Initialize");
            _tfPoolParent = new GameObject("Pools").transform;
            _tfPoolParent.transform.position = new Vector3(10000, 10000, 10000);

            await LoadCharacterSkins();

            var environment = EnvironmentManager.GetEnvironment();
            _baseUrl = environment.ApiUrl + "/lands/CustomizeAssets3";
            _downloadProvider = new DownloadProvider(_zipLoader);
        }

        async UniTask LoadCharacterSkins()
        {
            foreach (var model in _modelConfig.CharacterSkins)
            {
                var prefabModel = await _remoteLoader.LoadAssetAsync<GameObject>(model.Reference);
                prefabModel.SetActive(false);
                _dictCharacters.Add(model.ID, new ObjectPool<Transform>(prefabModel.transform));
            }
        }

        public GModel CreateCharacter(UserData newData)
        {
            GModel model = new GModel();
            model = _dictCharacters[newData.gender.ToString()].Get().GetComponent<GModel>();
            model.Setup(newData.gender);
            foreach (var equip in newData.equipments)
            {
                CreateProp(model, _configController.GetPropData(newData.gender, equip.type, equip.id), equip.color);
            }
            return model;
        }

        public void RemoveCharacter(GModel model)
        {
            ClearProps(model);
            _dictCharacters[model.Gender.ToString()].Store(model.transform);
        }

        public async UniTask StoreLand(List<SubLandEntity> entities)
        {
            foreach (var entity in entities)
            {
                while (!entity.Loaded)
                    await UniTask.Yield();
                entity.Transform.SetParent(_tfPoolParent);
                _poolLandModels[entity.Key].Store(entity.Transform);
            }
        }

        public async void PreloadModels(Land landData, string rootPath)
        {
            Dictionary<string, RequestRemoteAsset> newAssets = new Dictionary<string, RequestRemoteAsset>();
            var sceneData = landData.scene;
            foreach (var entity in sceneData.entities)
            {
                // if (entity.Key.Equals("e-43"))
                // {
                foreach (var componentId in entity.Value.components)
                {
                    var componentData = sceneData.components[componentId];
                    if (componentData.type == "GLTFShape")
                    {
                        var assetData = sceneData.assets[componentData.data.assetId];
                        foreach (var content in assetData.contents)
                        {
                            var extension = System.IO.Path.GetExtension(content.Key);
                            if (extension.Contains("glb") || extension.Contains("gltf"))
                            {
                                var keyAsset = content.Key;
                                if (!_poolLandModels.ContainsKey(keyAsset) && !_cacheLandAssets.ContainsKey(keyAsset) && !newAssets.ContainsKey(keyAsset))
                                {
                                    newAssets.Add(keyAsset, new RequestRemoteAsset($"{rootPath}/{assetData.id}/{keyAsset}", sceneData.components[componentId].data?.values));
                                }
                            }
                        }
                    }
                }
                // }
            }

            foreach (var asset in newAssets)
            {
                _cacheLandAssets.Add(asset.Key, asset.Value);
                var importer = new GLTFast.GltfImport(logger: new ConsoleLogger(), downloadProvider: _downloadProvider);
                var settings = new ImportSettings() { GenerateMipMaps = false, AnisotropicFilterLevel = 3, NodeNameMethod = GLTFast.NameImportMethod.OriginalUnique };
                var result = asset.Value.URL.StartsWith("http") ? await importer.Load(new System.Uri(asset.Value.URL), settings) : await importer.Load(asset.Value.URL, settings);
                if (result)
                {
                    var tfAsset = new GameObject(string.Format($"Preload_{asset.Key}"));
                    tfAsset.transform.SetParent(_tfPoolParent);
                    try
                    {
                        if (await importer.InstantiateSceneAsync(tfAsset.transform))
                        {
                            ProcessCollider(tfAsset.transform);
                            tfAsset.SetActive(false);
                            _poolLandModels.Add(asset.Key, new ObjectPool<Transform>(tfAsset.transform));
                            _cacheLandAssets.Remove(asset.Key);
                        }
                        else
                        {
                            _logger.Error($"[ModelController] Instantiate Model [{asset.Key}] failed!");
                            _poolLandModels.Add(asset.Key, new ObjectPool<Transform>(new GameObject().transform));
                            _cacheLandAssets.Remove(asset.Key);
                        }
                    }
                    catch
                    {
                        _logger.Error($"[ModelController] Exception: [{asset.Key}], [tfAsset = {tfAsset.transform == null}] failed!");
                        _poolLandModels.Add(asset.Key, new ObjectPool<Transform>(new GameObject().transform));
                        _cacheLandAssets.Remove(asset.Key);
                    }
                }
                else
                {
                    _logger.Error($"[ModelController] Load Model [{asset.Key}] failed!");
                    _poolLandModels.Add(asset.Key, new ObjectPool<Transform>(new GameObject().transform));
                    _cacheLandAssets.Remove(asset.Key);
                }
            }
        }

        public async UniTask<LandEntity> CreateLand(LandData data)
        {
            var zipData = await _zipLoader.LoadAssetAsync<ZipArchive>(data.resource);
            var posLand = new Vector3(data.left + data.metadata.offset[0], 0, data.top + data.metadata.offset[1]);
            var transformPath = new UrlTransformAll().Transform(data.resource);

            var sceneStream = new StreamReader(zipData.GetEntry("builder.json").Open());
            var jsonReader = new JsonTextReader(sceneStream);
            JsonSerializer serializer = new JsonSerializer();
            var builderData = serializer.Deserialize<Land>(jsonReader);
            PreloadModels(builderData, transformPath);

            var landEntity = new LandEntity(data.id, data);
            var sceneData = builderData.scene;
            landEntity.RootTransform = new GameObject($"Land_{data.id}").transform;
            landEntity.RootTransform.position = posLand;

            foreach (var entityGroupData in builderData.scene.entities)
            {
                Transform tfEntity = null;
                var entityData = entityGroupData.Value;
                var comModels = (from com in sceneData.components.Values where com.type == "GLTFShape" && entityData.components.Contains(com.id) select com).ToList();
                if (comModels.Count > 0)
                {
                    foreach (var content in sceneData.assets[comModels[0].data.assetId].contents)
                    {
                        var extension = System.IO.Path.GetExtension(content.Key);
                        if (extension.Contains("glb") || extension.Contains("gltf"))
                        {
                            var keyAsset = content.Key;
                            var subEntity = new SubLandEntity(keyAsset);
                            landEntity.Entities.Add(subEntity);
                            tfEntity = await CreateAsset(keyAsset);
                            if (tfEntity == null) tfEntity = new GameObject(entityData.name).transform;
                            tfEntity.name = entityData.name;
                            tfEntity.SetParent(landEntity.RootTransform);
                            subEntity.Transform = tfEntity;
                            subEntity.Loaded = true;
                        }
                    }
                }
                else
                {
                    tfEntity = new GameObject(entityData.name).transform;
                }
                tfEntity.SetParent(landEntity.RootTransform);

                foreach (var componentId in entityData.components)
                {
                    var componentData = sceneData.components[componentId];
                    if (componentData != null)
                    {
                        if (componentData.type == "Transform")
                        {
                            tfEntity.localPosition = componentData.data.position;
                            tfEntity.localRotation = componentData.data.rotation;
                            tfEntity.localScale = componentData.data.scale;
                        }
                    }
                }
            }

            // Check Land
            var scriptData = zipData.GetEntry("game.lua")?.Open() ?? null;
            if (scriptData != null)
            {
                var scripStream = new StreamReader(scriptData);
                string script = scripStream.ReadToEnd();
                _luaFactory.Create(script, landEntity.RootTransform.gameObject);
                Debug.Log($"[DNguyen] Load script: Land_{data.id}");
            }

            return landEntity;
        }
        public async UniTask<LocalLand> LoadLocalModel(ModelLocal model)
        {
            var landEntity = new LocalLand(model);
            landEntity.RootTransform = new GameObject(model.ID).transform;
            landEntity.RootTransform.position = model.Position;
            var resourceRequest = Resources.LoadAsync<GameObject>(model.ID);
            await resourceRequest.ToUniTask();

            if (resourceRequest.asset == null)
            {
                Debug.LogError("Failed to load resource: " + model.ID);
                return null;
            }
            var land = GameObject.Instantiate(resourceRequest.asset as GameObject);
            land.transform.SetParent(landEntity.RootTransform);
            land.transform.localPosition = Vector3.zero;
            return landEntity;
        }

        async UniTask<Transform> CreateAsset(string keyAsset)
        {
            if (_poolLandModels.ContainsKey(keyAsset))
            {
                return _poolLandModels[keyAsset].Get();
            }
            else if (_cacheLandAssets.ContainsKey(keyAsset))
            {
                while (!_poolLandModels.ContainsKey(keyAsset))
                    await UniTask.Yield();
                return _poolLandModels[keyAsset].Get();
            }
            else
            {
                return null;
            }
        }

        public async Task<bool> LoadSingleModel(GameObject root, string fullPath)
        {
            var gltf = new GLTFast.GltfImport(logger: new ConsoleLogger(), downloadProvider: _downloadProvider);
            var settings = new ImportSettings
            {
                GenerateMipMaps = false,
                AnisotropicFilterLevel = 3,
                NodeNameMethod = GLTFast.NameImportMethod.OriginalUnique
            };
            try
            {
                var success = fullPath.StartsWith("http") ? await gltf.Load(new System.Uri(fullPath), settings) : await gltf.Load(fullPath, settings);

                if (success && root.transform.activeSelf())
                {
                    if (await gltf.InstantiateSceneAsync(root.transform))
                    {
                        ProcessCollider(root.transform);
                    }
                }
                return success;
            }
            catch (System.Exception e)
            {
                Debug.LogErrorFormat("[LoadSingleModel] fail: {0}", e);
                return false;
            }
        }

        public async Task<Texture> LoadSingleTexture(string path)
        {
            try
            {
                var texture = await _downloadProvider.RequestTexture(new System.Uri(path), true);
                return texture?.Texture;
            }
            catch (System.Exception e)
            {
                Debug.LogErrorFormat("[LoadSingleTexture] error, path={0}, error={1}", path, e.ToString());
                return null;
            }
        }

        void ProcessCollider(Transform obj)
        {
            if (obj.name.ToLower().EndsWith("_collider"))
            {
                var meshRenderer = obj.GetComponent<MeshRenderer>();
                if (meshRenderer)
                {
                    var collider = obj.gameObject.GetComponent<MeshCollider>();
                    if (collider == null)
                    {
                        obj.gameObject.AddComponent<MeshCollider>();
                    }
                    meshRenderer.enabled = false;
                }
                else
                {
                    obj.gameObject.SetActive(false);
                }
            }
            var c = obj.childCount;
            for (var i = 0; i < c; i++)
            {
                ProcessCollider(obj.GetChild(i));
            }
        }
    }
}