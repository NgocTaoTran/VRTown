using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRTown.Scene;

namespace VRTown.Game.UI
{
    public class ChangeNameUI : UIController
    {
        [SerializeField] TMP_InputField _inputDisplayName;
        [SerializeField] Button _btnOK;

        System.Action<string> _onOK;

        public void Setup(string currentName, System.Action<string> onOK)
        {
            _inputDisplayName.text = currentName;
            _onOK = onOK;
        }

        public void OnNameChanged()
        {
            _btnOK.interactable = !string.IsNullOrEmpty(_inputDisplayName.text);
        }

        public void TouchedOK()
        {
            UIManager.Instance.ReleaseUI(this, true);
            _onOK?.Invoke(_inputDisplayName.text);
        }
    }
}