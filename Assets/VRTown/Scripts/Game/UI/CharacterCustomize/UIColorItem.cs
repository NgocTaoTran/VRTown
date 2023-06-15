using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRTown.Model;

namespace VRTown.Game.UI
{
    public class UIColorData
    {
        public int ColorID;
        public bool IsCurrent;
        public ToggleGroup Group;
        public System.Action<int> OnSelected;

        public UIColorData(int id, bool isCurrent, ToggleGroup group, System.Action<int> onChanged)
        {
            ColorID = id;
            Group = group;
            IsCurrent = isCurrent;
            OnSelected = onChanged;
        }
    }

    public class UIColorItem : MonoBehaviour, Scrmizu.IInfiniteScrollItem
    {
        [SerializeField] Image _imgColor;

        protected Toggle Toggle
        {
            get
            {
                if (_toggle == null) _toggle = GetComponent<Toggle>();
                return _toggle;
            }
        }

        Toggle _toggle = null;

        UIColorData Data;

        bool _isInit = false;

        public void Hide()
        {
            _isInit = false;
            this.gameObject.SetActive(false);
        }

        public void UpdateItemData(object data)
        {
            if (!(data is UIColorData)) return;
            Data = data as UIColorData;

            gameObject.SetActive(true);
            _imgColor.color = C.ColorConfig.Colors[Data.ColorID].ToColor();
            Toggle.group = Data.Group;
            Toggle.SetIsOnWithoutNotify(Data.IsCurrent);
            _isInit = true;
        }

        public void OnSelected()
        {
            if (!_isInit) return;
            if (Toggle.isOn)
                Data?.OnSelected(Data.ColorID);
        }
    }
}