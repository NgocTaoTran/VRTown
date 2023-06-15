using System.Linq;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRTown.Model;

namespace VRTown.Game.UI
{
    public class UIBottomView : UICustomizeView
    {
        [Header("Items")]
        [SerializeField] TMP_Text _textTitleItem;
        [SerializeField] Scrmizu.InfiniteScrollRect _scrollItems;
        [SerializeField] ToggleGroup _groupItem;

        System.Action<PropData, int> _onBottomChanged;

        PropData _currentProp;

        bool isInit = false;
        UserData _userData;

        public override void Setup(UIViewData data)
        {
            _onBottomChanged = (data.Parameters[C.KeyConfigs.KEY_ACTION_ITEM_CHANGED] as System.Action<PropData, int>);
            _userData = data.Parameters[C.KeyConfigs.KEY_VALUE_USER_DATA] as UserData;
            _scrollItems.SetItemData(GetDataStyle(PropType.pants));
            isInit = true;
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
        public void OnItemChanged(PropData propData)
        {
            _currentProp = propData;
            _onBottomChanged?.Invoke(_currentProp, -1);
        }
        #endregion Callbacks

        public override void ReleaseView()
        {

        }
    }
}