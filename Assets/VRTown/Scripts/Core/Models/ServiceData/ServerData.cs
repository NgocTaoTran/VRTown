using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

namespace VRTown.Model
{
    [System.Serializable]
    public class CharacterSetting
    {
        [JsonConverter(typeof(StringEnumConverter))] public GenderType gender;
        public Dictionary<string, int> custom;
        public Dictionary<string, int> colors;

        public CharacterSetting()
        {
            gender = GenderType.male;
            custom = new Dictionary<string, int>();
            colors = new Dictionary<string, int>();
        }

        public int GetColorID(PropType type)
        {
            foreach (var color in colors)
            {
                if (color.Key.Contains(type.ToString().ToLower()))
                    return color.Value;
            }
            return -1;
        }

        public int GetPropID(PropType type)
        {
            foreach (var item in custom)
            {
                if (item.Key.Contains(type.ToString().ToLower()))
                    return item.Value;
            }
            return 1;
        }

        public void SetSkin(int colorID)
        {
            colors["body."] = colorID;
        }

        public void SetPropData(PropData propData)
        {
            var key = $"{propData.category}.{propData.type}";
            if (!custom.ContainsKey(key))
                custom.Add(key, 0);
            custom[key] = propData.id;
        }

        public void SetPropColor(PropData propData, int colorId)
        {
            var key = $"{propData.category}.{propData.type}";
            if (!colors.ContainsKey(key))
                colors.Add(key, 0);
            colors[key] = propData.id;
        }
    }
    
    [System.Serializable]
    public class UserMetaData
    {
        public double[] position;
        public UserData user_data;
    }
}