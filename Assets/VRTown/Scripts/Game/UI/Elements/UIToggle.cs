using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace VRTown.Game.UI
{
    [RequireComponent(typeof(Toggle))]
    public class UIToggle : MonoBehaviour
    {
        public Toggle Toggle
        {
            get
            {
                if (_toggle == null)
                    _toggle = GetComponent<Toggle>();
                return _toggle;
            }
        }

        Toggle _toggle = null;

        [Header("Toggle States")]
        [SerializeField] private GameObject _groupOn;
        [SerializeField] private GameObject _groupOff;

        void Update()
        {
            if (Toggle == null) return;
            if (_groupOn != null) _groupOn.SetActive(Toggle.isOn);
            if (_groupOff != null) _groupOff.SetActive(!Toggle.isOn);
        }
    }
}