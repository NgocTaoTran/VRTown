using System.Linq;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRTown.Model;

namespace VRTown.Game.UI
{
    public class UITopView : UICustomizeView
    {
        [Header("Items")]
        [SerializeField] TMP_Text _textTitleItem;
        [SerializeField] Scrmizu.InfiniteScrollRect _scrollItems;
        [SerializeField] ToggleGroup _groupItem;

        System.Action<PropData, int> _onTopChanged;

        PropData _currentProp;

        bool isInit = false;
        UserData _userData;

        public override void Setup(UIViewData data)
        {
            _onTopChanged = (data.Parameters[C.KeyConfigs.KEY_ACTION_ITEM_CHANGED] as System.Action<PropData, int>);
            _userData = data.Parameters[C.KeyConfigs.KEY_VALUE_USER_DATA] as UserData;
            _scrollItems.SetItemData(GetDataStyle(PropType.shirt));
            isInit = true;
        }

        #region Datas
        public List<UIStyleData> GetDataStyle(PropType type)
        {
            var propDatas = GHelper.Config.GetProps(_userData.gender, type);
            var equipmentData = _userData.equipments.Find(val => val.type == PropType.top);
            List<UIStyleData> styleDatas = new List<UIStyleData>();
            foreach (var propData in propDatas)
            {
                styleDatas.Add(new UIStyleData(propData, propData.id == ((equipmentData != null) ? equipmentData.id : -1), _groupItem, OnItemChanged));
            }
            return styleDatas;
        }
        #endregion Datas

        #region Callbacks
        public void OnItemChanged(PropData propData)
        {
            _currentProp = propData;
            _onTopChanged?.Invoke(_currentProp, -1);
        }
        #endregion Callbacks

        public override void ReleaseView()
        {

        }
    }
}