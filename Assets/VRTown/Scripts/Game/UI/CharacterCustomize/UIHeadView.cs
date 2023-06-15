using System.Linq;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRTown.Model;

namespace VRTown.Game.UI
{
    public class UIHeadView : UICustomizeView
    {
        public enum HeadSubTab
        {
            Bread,
            Hair
        }

        [System.Serializable]
        public class UIHeadSubView
        {
            public HeadSubTab Type;
            public Toggle Toggle;
            public string Name;
        }

        [Header("Views")]
        [SerializeField] List<UIHeadSubView> _subViews;

        // [Header("UI")]
        [Header("Colors")]
        [SerializeField] TMP_Text _textTitleColor;
        [SerializeField] Scrmizu.InfiniteScrollRect _scrollColors;
        [SerializeField] ToggleGroup _groupColor;

        [Header("Items")]
        [SerializeField] TMP_Text _textTitleItem;
        [SerializeField] Scrmizu.InfiniteScrollRect _scrollItems;
        [SerializeField] ToggleGroup _groupItem;

        System.Action<PropData, int> _onHeadChanged;

        Dictionary<Toggle, UIHeadSubView> _dictToggles = new Dictionary<Toggle, UIHeadSubView>();

        HeadSubTab _curSubView = HeadSubTab.Bread;

        int _currentColor;
        PropData _currentProp;

        bool isInit = false;
        UserData _userData;

        public override void Setup(UIViewData data)
        {
            LoadReferences();
            _onHeadChanged = (data.Parameters[C.KeyConfigs.KEY_ACTION_ITEM_CHANGED] as System.Action<PropData, int>);
            _userData = data.Parameters[C.KeyConfigs.KEY_VALUE_USER_DATA] as UserData;
            ChangeSubView(_curSubView, true);
            isInit = true;
        }

        void OnEnable()
        {
            GHelper.Localization.LocalizationChanged += onLanguageChanged;
        }

        void OnDisable()
        {
            GHelper.Localization.LocalizationChanged -= onLanguageChanged;
        }

        void LoadReferences()
        {
            _dictToggles.Clear();
            foreach (var view in _subViews)
                _dictToggles.Add(view.Toggle, view);
        }

        void ChangeSubView(HeadSubTab newSubTab, bool ignore = false)
        {
            if (!ignore && newSubTab == _curSubView) return;
            _curSubView = newSubTab;
            switch (_curSubView)
            {
                case HeadSubTab.Bread:
                    {
                        var equipmentData = _userData.equipments.Find(val => val.type == PropType.beard);
                        _currentColor = equipmentData != null ? equipmentData.color : -1;
                        _currentProp = GHelper.Config.GetPropData(_userData.gender, PropType.beard, _currentColor);
                        _scrollColors.SetItemData(GetDataColors(_currentColor));
                        _scrollItems.SetItemData(GetDataStyle(PropType.beard));
                        break;
                    }

                case HeadSubTab.Hair:
                    {
                        var equipmentData = _userData.equipments.Find(val => val.type == PropType.hair);
                        _currentColor = equipmentData != null ? equipmentData.color : -1;
                        _currentProp = GHelper.Config.GetPropData(_userData.gender, PropType.hair, _currentColor);
                        _scrollColors.SetItemData(GetDataColors(_currentColor));
                        _scrollItems.SetItemData(GetDataStyle(PropType.hair));
                        break;
                    }
            }
        }

        #region Datas
        public List<UIColorData> GetDataColors(int currentValue)
        {
            List<UIColorData> dataColors = new List<UIColorData>();
            for (int i = 0; i < C.ColorConfig.Colors.Length; i++)
                dataColors.Add(new UIColorData(i, i == currentValue, _groupColor, OnColorChanged));
            return dataColors;
        }

        public List<UIStyleData> GetDataStyle(PropType type)
        {
            var equipmentData = _userData.equipments.Find(val => val.type == type);
            var propDatas = GHelper.Config.GetProps(_userData.gender, type);
            List<UIStyleData> styleDatas = new List<UIStyleData>();
            foreach (var propData in propDatas)
            {
                styleDatas.Add(new UIStyleData(propData, propData.id == _currentColor, _groupItem, OnItemChanged));
            }
            return styleDatas;
        }
        #endregion Datas

        #region Callbacks
        public void OnSubViewChanged(Toggle toggle)
        {
            if (!isInit) return;
            if (toggle.isOn)
            {
                var subView = _dictToggles[toggle];
                _textTitleColor.text = GHelper.Localization.Localize<string>($"TXT_{subView.Type.ToString().ToUpper()}_COLOR");
                _textTitleItem.text = GHelper.Localization.Localize<string>($"TXT_{subView.Type.ToString().ToUpper()}_STYLE");
                ChangeSubView(_dictToggles[toggle].Type);
            }
        }

        public void OnColorChanged(int colorId)
        {
            _currentColor = colorId;
            _onHeadChanged?.Invoke(_currentProp, _currentColor);
        }

        public void OnItemChanged(PropData propData)
        {
            _currentProp = propData;
            _onHeadChanged?.Invoke(_currentProp, _currentColor);
        }
        #endregion Callbacks

        public override void ReleaseView()
        {

        }

        void onLanguageChanged()
        {
            _textTitleColor.text = GHelper.Localization.Localize<string>($"TXT_{_curSubView.ToString().ToUpper()}_COLOR");
            _textTitleItem.text = GHelper.Localization.Localize<string>($"TXT_{_curSubView.ToString().ToUpper()}_STYLE");
        }
    }
}