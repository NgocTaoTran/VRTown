using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using VRTown.Model;

namespace VRTown.Game.UI
{
    public class UIStyleData
    {
        public PropData PropData;
        public bool IsCurrent;
        public ToggleGroup Group;
        public System.Action<PropData> OnSelected;

        public UIStyleData(PropData data, bool isCurrent, ToggleGroup group, System.Action<PropData> onChanged)
        {
            IsCurrent = isCurrent;
            PropData = data;
            Group = group;
            OnSelected = onChanged;
        }
    }

    public class UIStyleItem : MonoBehaviour, Scrmizu.IInfiniteScrollItem
    {
        [SerializeField] Image _imgIcon;
        [SerializeField] Transform _tfLoading;

        protected Toggle Toggle
        {
            get
            {
                if (_toggle == null) _toggle = GetComponent<Toggle>();
                return _toggle;
            }
        }

        Toggle _toggle = null;

        UIStyleData Data = null;

        bool _isInit = false;
        public void Hide()
        {
            _isInit = false;
            this.gameObject.SetActive(false);
        }

        public void UpdateItemData(object data)
        {
            if (!(data is UIStyleData)) return;
            Data = data as UIStyleData;
            gameObject.SetActive(true);

            LoadIcon();
            Toggle.SetIsOnWithoutNotify(Data.IsCurrent);
            _isInit = true;
            GameUtils.Delay(0.1f, () => Toggle.group = Data.Group);
        }

        async void LoadIcon()
        {
            _tfLoading.gameObject.SetActive(true);
            _imgIcon.gameObject.SetActive(false);
            _imgIcon.sprite = await GHelper.Config.LoadTextureAsync(Data.PropData.path);
            _imgIcon.SetNativeSize();
            _imgIcon.gameObject.SetActive(true);
            _tfLoading.gameObject.SetActive(false);

        }

        public void OnSelected()
        {
            if (!_isInit) return;
            if (Toggle.isOn)
                Data?.OnSelected(Data.PropData);
        }
    }
}