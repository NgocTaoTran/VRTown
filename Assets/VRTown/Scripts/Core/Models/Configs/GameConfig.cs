using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;

namespace VRTown.Model
{
    [System.Serializable]
    public class ModelData
    {
        public string ID;
        public AssetReference Reference;
    }

    [System.Serializable]
    public class ModelConfig
    {
        public List<ModelData> CharacterSkins;
    }

    [System.Serializable]
    public class InputConfig
    {
        public InputActionAsset InputAction;
    }
}