using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTown.Model;
using VRTown.Utilities;
using static VRTown.Model.C;

namespace VRTown.Game
{
    public struct ModelMap
    {
        public string Key;
        public Renderer Transform;

        public ModelMap(string key, Renderer tran)
        {
            Key = key;
            Transform = tran;
        }
    }

    public partial class GModel : MonoBehaviour
    {

        [SerializeField] SkinnedMeshRenderer _mainMesh;
        [SerializeField] public bool SupportProps = true;
        
        public GenderType Gender { get; set; }

        Dictionary<PropType, ModelMap> _partMaps = new Dictionary<PropType, ModelMap>();

        public void Setup(GenderType gender)
        {
            Gender = gender;
        }

        public bool ContainProp(PropData data)
        {
            return _partMaps.ContainsKey(data.type) && _partMaps[data.type].Key == data.GetKey();
        }

        public Renderer AddProp(PropData propData, Renderer renderer)
        {
            Renderer oldProp = null;
            if (_partMaps.ContainsKey(propData.type))
            {
                oldProp = _partMaps[propData.type].Transform;
                _partMaps.Remove(propData.type);
            }

            renderer.transform.SetParent(this.transform);
            _partMaps.Add(propData.type, new ModelMap(propData.GetKey(), renderer));

            if (renderer.GetType() == typeof(SkinnedMeshRenderer))
            {
                renderer.transform.localPosition = Vector3.zero;
                renderer.transform.localScale = Vector3.one;
                renderer.transform.localRotation = Quaternion.identity;
                (renderer as SkinnedMeshRenderer).bones = _mainMesh.bones;
                (renderer as SkinnedMeshRenderer).rootBone = _mainMesh.rootBone;
            }
            else if (renderer.GetType() == typeof(MeshRenderer))
            {
                var propTran = renderer.GetComponent<PropTransform>();
                var tfItem = transform.FindInChildren(propTran.ParentName);
                renderer.transform.SetParent(tfItem);
                renderer.transform.localPosition = propTran.Position;
                renderer.transform.localRotation = propTran.Rotation;
                renderer.transform.localScale = propTran.Scale;
            }
            return oldProp;
        }

        public void ClearProps(GModel model, Dictionary<string, ObjectPool<Renderer>> poolProps, Transform tfStore)
        {
            foreach (var partItem in _partMaps)
            {
                poolProps[partItem.Value.Key].Store(partItem.Value.Transform);
            }
            _partMaps.Clear();
        }

        public void SetSkinColor(int colorId)
        {
            var renderers = GetComponentsInChildren<Renderer>();
            foreach (var render in renderers)
            {
                if (render.sharedMaterial.name == "CharacterSkinMaterial" || render.sharedMaterial.name == "SkinBase")
                {
                    var block = new MaterialPropertyBlock();
                    block.SetColor("baseColorFactor", ColorConfig.Colors[colorId].ToColor());
                    render.SetPropertyBlock(block);
                }
            }
        }

        public void SetPropColor(PropType type, int colorId)
        {
            if (!_partMaps.ContainsKey(type)) return;
            var renderers = _partMaps[type].Transform.GetComponentsInChildren<Renderer>();
            foreach (var render in renderers)
            {
                if (render.sharedMaterial.name == "CharacterSkinMaterial" || render.sharedMaterial.name == "SkinBase")
                {
                    var block = new MaterialPropertyBlock();
                    block.SetColor("baseColorFactor", ColorConfig.Colors[colorId].ToColor());
                    render.SetPropertyBlock(block);
                }
            }
        }
    }
}