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
    public partial class ModelController : IModelController
    {
        Dictionary<string, ObjectPool<Renderer>> _poolProps = new Dictionary<string, ObjectPool<Renderer>>();
        List<string> _propLoadings = new List<string>();

        public void ChangeSkin(ref GModel model, UserData newData)
        {
            if (model.Gender != newData.gender)
            {
                var tfParent = model.transform.parent;
                var localPos = model.transform.localPosition;
                var localRotation = model.transform.localRotation;
                var scale = model.transform.localScale;
                var oldTransform = model.transform;

                ClearProps(model);
                _dictCharacters[model.Gender.ToString()].Store(model.transform);

                model = CreateCharacter(newData);
                model.transform.SetParent(tfParent);
                model.transform.localPosition = localPos;
                model.transform.localRotation = localRotation;
                model.transform.localScale = scale;
            }
        }

        public void UpdateEquipments(GModel model, UserData newData)
        {
            foreach (var equip in newData.equipments)
            {
                CreateProp(model, _configController.GetPropData(newData.gender, equip.type, equip.id), equip.color);
            }
        }

        public async void CreateProp(GModel model, PropData data, int colorId = -1)
        {
            if (data == null || !model.SupportProps) return;
            if (!model.ContainProp(data))
            {
                var propItem = await GetProp(data);
                propItem.gameObject.setLayerRecursively(model.gameObject.layer);
                var oldProp = model.AddProp(data, propItem);
                if (oldProp != null)
                {
                    var renderer = oldProp.GetComponent<Renderer>();
                    if (renderer != null)
                        _poolProps[data.GetKey()].Store(renderer);
                }
            }

            if (colorId >= 0)
                ChangeColorProp(model, data.type, colorId);
        }

        public void ChangeColorProp(GModel model, PropType type, int newColor)
        {
            if (newColor < 0 || GHelper.Config.PropNoColor.Contains(type)) return;
            model.SetPropColor(type, newColor);
        }

        public void ChangeSkinColor(GModel model, int newColor = -1)
        {
            if (newColor < 0) return;
            model.SetSkinColor(newColor);
        }

        public void ClearProps(GModel model)
        {
            model.ClearProps(model, _poolProps, _tfPoolParent);
        }

        async UniTask<Renderer> GetProp(PropData propData)
        {
            if (!_propLoadings.Contains(propData.GetKey()) && !_poolProps.ContainsKey(propData.GetKey()))
            {
                _logger.Log("[Model] Create New Prop");
                var gltfImporter = new GLTFast.GltfImport(logger: new ConsoleLogger());
                var settings = new ImportSettings
                {
                    GenerateMipMaps = false,
                    AnisotropicFilterLevel = 3,
                    NodeNameMethod = GLTFast.NameImportMethod.OriginalUnique
                };

                _propLoadings.Add(propData.GetKey());

                await gltfImporter.Load($"{_baseUrl}/{propData.path}", settings);

                GameObject prefab = new GameObject($"Prefab_{propData.GetKey()}");
                prefab.transform.SetParent(_tfPoolParent);
                try
                {
                    if (await gltfImporter.InstantiateSceneAsync(prefab.transform))
                    {
                        var skinMesh = prefab.GetComponentInChildren<Renderer>();
                        var propTran = skinMesh.gameObject.AddComponent<PropTransform>();
                        propTran.ParentName = skinMesh.transform.parent.name;
                        propTran.SetupTransform(skinMesh.transform);

                        skinMesh.transform.SetParent(_tfPoolParent);
                        skinMesh.gameObject.SetActive(false);
                        _poolProps.Add(propData.GetKey(), new ObjectPool<Renderer>(skinMesh));
                        _propLoadings.Remove(propData.GetKey());
                        GameObject.Destroy(prefab);
                    }
                    else
                    {
                        var renderEmpty = prefab.AddComponent<Renderer>();
                        _poolProps.Add(propData.GetKey(), new ObjectPool<Renderer>(renderEmpty));
                        _propLoadings.Remove(propData.GetKey());
                        _logger.Error($"[ModelController] Load Prop [{propData.GetKey()}] failed!");
                    }
                }
                catch
                {
                    var renderEmpty = prefab.AddComponent<Renderer>();
                    _poolProps.Add(propData.GetKey(), new ObjectPool<Renderer>(renderEmpty));
                    _propLoadings.Remove(propData.GetKey());
                    _logger.Error($"[ModelController] Exception load prop [{propData.GetKey()}] failed!");
                }
            }

            while (!_poolProps.ContainsKey(propData.GetKey()))
                await UniTask.Yield();
            return _poolProps[propData.GetKey()].Get();
        }
    }
}