using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTown.Scene;

namespace VRTown.Game.UI
{
    public class HomeUI : UIController
    {
        [SerializeField] UICharacter _tfCharacter;

        System.Action<string> _onTouchJoin;

        public void Setup(System.Action<string> onTouchJoin)
        {
            _onTouchJoin = onTouchJoin;
        }

        public void TouchedJoin(string mapName)
        {
            _onTouchJoin?.Invoke(mapName);
        }
    }
}