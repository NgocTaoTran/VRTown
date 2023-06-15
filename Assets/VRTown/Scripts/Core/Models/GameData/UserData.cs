using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

namespace VRTown.Model
{
    public class UserData
    {
        [JsonConverter(typeof(StringEnumConverter))] public CharacterType type;
        public string id;
        public string name;
        public int skin;
        [JsonConverter(typeof(StringEnumConverter))] public GenderType gender;
        public List<EquipmentData> equipments;
        public UserData(string id)
        {
            this.id = id;
        }
    }

    [System.Serializable]
    public class EquipmentData
    {
        public int id;
        public int color;
        [JsonConverter(typeof(StringEnumConverter))] public PropType type;

        public EquipmentData()
        {

        }

        public EquipmentData(int id, int color, PropType type)
        {
            this.id = id;
            this.color = color;
            this.type = type;
        }
    }

    [System.Serializable]
    public class UserJoinVoiceChat
    {
        public string userName;
    }
    public class UserLeaveVoiceChat
    {
        public string userName;
    }
}