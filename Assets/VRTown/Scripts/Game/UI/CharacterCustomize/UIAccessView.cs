using System.Linq;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRTown.Model;

namespace VRTown.Game.UI
{
    public class UIAccessView : UICustomizeView
    {
        public enum AccessSubTab
        {
            Glasses,
            Hat,
            Bag
        }

        [System.Serializable]
        public class UIAccessSubView
        {
            public AccessSubTab Type;
            public Toggle Toggle;
            public string Name;
        }

        [Header("Views")]
        [SerializeField] List<UIAccessSubView> _subViews;

        [Header("Items")]
        [SerializeField] TMP_Text _textTitleItem;
        [SerializeField] Scrmizu.InfiniteScrollRect _scrollItems;
        [SerializeField] ToggleGroup _groupItem;

        System.Action<PropData, int> _onAccessChanged;

        Dictionary<Toggle, UIAccessSubView> _dictToggles = new Dictionary<Toggle, UIAccessSubView>();

        AccessSubTab _curSubView = AccessSubTab.Glasses;

        PropData _currentProp;

        bool isInit = false;
        UserData _userData;

        public override void Setup(UIViewData data)
        {
            LoadReferences();
            _onAccessChanged = (data.Parameters[C.KeyConfigs.KEY_ACTION_ITEM_CHANGED] as System.Action<PropData, int>);
            _userData = data.Parameters[C.KeyConfigs.KEY_VALUE_USER_DATA] as UserData;

            ChangeSubView(_curSubView, true);
            isInit = true;
        }

        void LoadReferences()
        {
            _dictToggles.Clear();
            foreach (var view in _subViews)
                _dictToggles.Add(view.Toggle, view);
        }

        void ChangeSubView(AccessSubTab newSubTab, bool ignore = false)
        {
            if (!ignore && newSubTab == _curSubView) return;
            _curSubView = newSubTab;
            switch (_curSubView)
            {
                case AccessSubTab.Glasses:
                    {
                        var equipmentData = _userData.equipments.Find(val => val.type == PropType.glasses);
                        _currentProp = GHelper.Config.GetPropData(_userData.gender, PropType.glasses, equipmentData != null ? equipmentData.id : -1);
                        _scrollItems.SetItemData(GetDataStyle(PropType.glasses));
                        break;
                    }

                case AccessSubTab.Hat:
                    {
                        var equipmentData = _userData.equipments.Find(val => val.type == PropType.hat);
                        _currentProp = GHelper.Config.GetPropData(_userData.gender, PropType.hat, equipmentData != null ? equipmentData.id : -1);
                        _scrollItems.SetItemData(GetDataStyle(PropType.hat));
                        break;
                    }

                case AccessSubTab.Bag:
                    {
                        var equipmentData = _userData.equipments.Find(val => val.type == PropType.bag);
                        _currentProp = GHelper.Config.GetPropData(_userData.gender, PropType.bag, equipmentData != null ? equipmentData.id : -1);
                        _scrollItems.SetItemData(GetDataStyle(PropType.bag));
                        break;
                    }
            }
        }

        #region Datas
        public List<UIStyleData> GetDataStyle(PropType type)
        {
            var equipmentData = _userData.equipments.Find(val => val.type == type);
            var propDatas = GHelper.Config.GetProps(_userData.gender, type);
            List<UIStyleData> styleDatas = new List<UIStyleData>();
            foreach (var propData in propDatas)
            {
                styleDatas.Add(new UIStyleData(propData, propData.id == (equipmentData != null ? equipmentData.id : -1), _groupItem, OnItemChanged));
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
                _textTitleItem.text = subView.Name + " Style";
                ChangeSubView(_dictToggles[toggle].Type);
            }
        }

        public void OnItemChanged(PropData propData)
        {
            _currentProp = propData;
            _onAccessChanged?.Invoke(_currentProp, -1);
        }
        #endregion Callbacks

        public override void ReleaseView()
        {

        }
    }
}