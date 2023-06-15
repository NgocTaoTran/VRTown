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
    public class UIAudienceItemData
    {
        public uint UID;
        public string UserName;
        public System.Action<uint, bool> OnShareMic;

        public UIAudienceItemData(uint uid, string name, System.Action<uint, bool> onShareMic)
        {
            UID = uid;
            UserName = name;
            OnShareMic = onShareMic;
        }
    }

    public class UIAudienceItem : MonoBehaviour, IInfiniteScrollItem
    {
        [SerializeField] TMP_Text _textName;
        [SerializeField] Toggle _toggle;

        UIAudienceItemData _data;
        bool _isInit = false;

        public void Hide()
        {
            _isInit = false;
            this.gameObject.SetActive(false);
        }

        public void UpdateItemData(object data)
        {
            if (!(data is UIAudienceItemData)) return;
            _data = data as UIAudienceItemData;

            gameObject.SetActive(true);
            _toggle.SetIsOnWithoutNotify(true);
            _textName.text = _data.UserName;
            _isInit = true;
        }

        public void TouchedShareMic(Toggle toggle)
        {
            if (!_isInit) return;
            _data.OnShareMic(_data.UID, toggle.isOn);
        }
    }
}