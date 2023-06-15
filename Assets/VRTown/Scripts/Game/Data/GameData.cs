using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTown.Model;

namespace VRTown.Game.Data
{
    public enum StateType
    {
        moves = 0,
        joins = 1,
        changeSkin = 2,
    }

    public class StateData
    {
        public List<CharacterData> members;
        public StateType type;
    }
}