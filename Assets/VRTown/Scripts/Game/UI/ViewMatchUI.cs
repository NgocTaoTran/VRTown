using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRTown.Scene;
using Nakama;
using System;

namespace VRTown.Game.UI
{
    public class ViewMatchUI : UIController
    {
        
        [SerializeField] GameObject _goMain;
        [SerializeField] GameObject _goMetamask;
        [SerializeField] GameObject _goEmail;
        [SerializeField] TextMeshProUGUI _promptTitle;
        [SerializeField] TextMeshProUGUI _promptInputTitle;
        [SerializeField] TMPro.TMP_InputField _promptInput;
        System.Action<string> _onTouchPlay;

        public void Setup(System.Action<string> onTouchPlay)
        {
            _onTouchPlay = onTouchPlay;
        }

        public void TouchedPlay()
        {
            _onTouchPlay?.Invoke(_promptInput.text);
        }
    }
}