using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

namespace VRTown.Model
{
    public enum PropCategory
    {
        head,
        top,
        bottom,
        shoes,
        accessories,
    }

    public enum PropType
    {
        beard,
        hair,
        shirt,
        pants,
        shoes,
        glasses,
        hat,
        bag,

        // OLD
        body,
        top,
        bottom,
    }

    [System.Serializable]
    public class PropData
    {
        public int id;
        public string name;
        [JsonConverter(typeof(StringEnumConverter))] public GenderType gender;
        public string path;
        [JsonConverter(typeof(StringEnumConverter))] public PropType type;
        [JsonConverter(typeof(StringEnumConverter))] public PropCategory category;

        private string _key = "";

        public string GetKey()
        {
            if (string.IsNullOrEmpty(_key)) _key = $"{gender}_{category.ToString()}_{type.ToString()}_{id}";
            return _key;
        }
    }
}