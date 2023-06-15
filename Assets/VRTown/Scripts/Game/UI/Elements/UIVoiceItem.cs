using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Scrmizu;
using VRTown.Model;
using Newtonsoft.Json;
using UnityEngine.UI;

namespace VRTown.Game.UI
{
    public class UIVoiceItemData
    {
        public uint UID;
        public string UserName;
        public System.Action<uint, bool> OnMute;

        public UIVoiceItemData(uint uid, string name, System.Action<uint, bool> onMute)
        {
            UID = uid;
            UserName = name;
            OnMute = onMute;
        }
    }

    public class UIVoiceItem : MonoBehaviour, IInfiniteScrollItem
    {
        [SerializeField] TMP_Text _textName;
        [SerializeField] Toggle _toggle;

        UIVoiceItemData _data;
        bool _isInit = false;

        public void Hide()
        {
            _isInit = false;
            this.gameObject.SetActive(false);
        }

        public void UpdateItemData(object data)
        {
            if (!(data is UIVoiceItemData)) return;
            _data = data as UIVoiceItemData;

            gameObject.SetActive(true);
            _toggle.SetIsOnWithoutNotify(true);
            _textName.text = _data.UserName;
            _isInit = true;
        }

        public void TouchedMute(Toggle toggle)
        {
            if (!_isInit) return;
            _data.OnMute(_data.UID, toggle.isOn);
        }
    }
}