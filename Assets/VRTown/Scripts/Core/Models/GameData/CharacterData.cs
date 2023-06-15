using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRTown.Model
{
    public enum GenderType
    {
        none = 0,
        male,
        female,
        anime
    }

    [System.Serializable]
    public enum CharacterType
    {
        User,
        Opponent,
        NPC
    }

    [System.Serializable]
    public class CharacterItem
    {
        public int id;
        public string path;
        public string name;
        public string category;
        public string type;
        public bool isTicked;
    }

    [System.Serializable]
    public class CharacterData
    {
        public string id;
        public string name;
        public float x;
        public float y;
        public float z;
        public float d;
        public string t;
        public float gx;
        public float gy;
        public float gz;
        public UserData c;
        public StateData a;

    }

    [System.Serializable]
    public class UserLeaveMatch
    {
       public  string id;
    }
}