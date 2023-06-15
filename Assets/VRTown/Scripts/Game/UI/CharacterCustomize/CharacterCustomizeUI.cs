using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRTown.Scene;
using Nakama;
using System;
using VRTown.Model;
using VRTown.Service;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace VRTown.Game.UI
{
    public enum CustomizeTab
    {
        Body,
        Head,
        Top,
        Bottom,
        Shoes,
        Access
    }

    [System.Serializable]
    public class UITabPage
    {
        public CustomizeTab Type;
        public Toggle Tab;
        public UICustomizeView View;
    }

    public class CharacterCustomizeUI : UIController
    {
        [SerializeField] UICharacter _tfCharacter;
        [SerializeField] List<UITabPage> _tabPages;

        System.Action _onTouchSetting;
        System.Action _onTouchInventory;
        System.Action _onTouchPlay;
        System.Action _onTouchVoiceList;

        IGameController _gameController = null;
        CustomizeTab _currentTab = CustomizeTab.Body;

        Dictionary<Toggle, CustomizeTab> _dictToggleToTabs = new Dictionary<Toggle, CustomizeTab>();
        Dictionary<CustomizeTab, UICustomizeView> _dictViews = new Dictionary<CustomizeTab, UICustomizeView>();

        bool _isInit = false;
        IModelController _modelController;
        UserData _userData = null;
        List<EquipmentData> _equipments = new List<EquipmentData>();
        System.Action<UserData> _onClosed = null;

        public void Setup(UserData userData, IGameController gameController, IModelController modelController, System.Action<UserData> onClosed)
        {
            _onClosed = onClosed;
            _userData = userData;
            _equipments = userData.equipments;
            _gameController = gameController;
            _modelController = modelController;

            _tfCharacter.Create(_modelController, _userData);

            _dictToggleToTabs.Clear();
            _dictViews.Clear();
            foreach (var item in _tabPages)
            {
                _dictToggleToTabs.Add(item.Tab, item.Type);
                _dictViews.Add(item.Type, item.View);
                item.View.gameObject.SetActive(false);
            }

            ChangePage(_currentTab, true);
            _isInit = true;
        }

        protected override void OnUIRemoved()
        {
            _tfCharacter.ClearProps();
        }

        public void OnPageChanged(Toggle toggle)
        {
            if (!_isInit) return;
            if (toggle.isOn)
            {
                ChangePage(_dictToggleToTabs[toggle]);
            }
        }

        void ChangePage(CustomizeTab newTab, bool ignore = false)
        {
            if (!ignore && newTab == _currentTab) return;

            _dictViews[_currentTab].ReleaseView();
            _dictViews[_currentTab].gameObject.SetActive(false);

            _currentTab = newTab;

            _dictViews[_currentTab].gameObject.SetActive(true);

            var viewData = new UIViewData();
            viewData.Parameters.Add(C.KeyConfigs.KEY_VALUE_USER_DATA, _userData);

            switch (_currentTab)
            {
                case CustomizeTab.Body:
                    {
                        viewData.Parameters.Add(C.KeyConfigs.KEY_ACTION_COLOR_CHANGED, (System.Action<int>)OnSkinChanged);
                        viewData.Parameters.Add(C.KeyConfigs.KEY_ACTION_GENDER_CHANGED, (System.Action<GenderType>)OnGenderChanged);
                        break;
                    }
                case CustomizeTab.Head:
                    {
                        viewData.Parameters.Add(C.KeyConfigs.KEY_ACTION_ITEM_CHANGED, (System.Action<PropData, int>)OnHeadChanged);
                        break;
                    }
                case CustomizeTab.Top:
                    {
                        viewData.Parameters.Add(C.KeyConfigs.KEY_ACTION_ITEM_CHANGED, (System.Action<PropData, int>)OnTopChanged);
                        break;
                    }
                case CustomizeTab.Bottom:
                    {
                        viewData.Parameters.Add(C.KeyConfigs.KEY_ACTION_ITEM_CHANGED, (System.Action<PropData, int>)OnBottomChanged);
                        break;
                    }
                case CustomizeTab.Shoes:
                    {
                        viewData.Parameters.Add(C.KeyConfigs.KEY_ACTION_ITEM_CHANGED, (System.Action<PropData, int>)OnBottomChanged);
                        break;
                    }
                case CustomizeTab.Access:
                    {
                        viewData.Parameters.Add(C.KeyConfigs.KEY_ACTION_ITEM_CHANGED, (System.Action<PropData, int>)OnAccessChanged);
                        break;
                    }
            }
            _dictViews[_currentTab].Setup(viewData);
        }
        
        public void TouchedClosed()
        {
            _userData.equipments = _equipments;
            _onClosed?.Invoke(_userData);
        }

        #region Body_View
        void OnSkinChanged(int colorId)
        {
            _userData.skin = colorId;
            _tfCharacter.ChangeSkinColor(colorId);
        }

        void OnGenderChanged(GenderType genderType)
        {
            _userData.gender = genderType;
            _tfCharacter.ChangeSkin(_userData);
        }
        #endregion Body_View

        #region Head_View
        void OnHeadChanged(PropData propData, int colorId)
        {
            if (propData == null) return;
            _userData.gender = propData.gender;
            _equipments.Add(new EquipmentData(propData.id, colorId, propData.type));
            _tfCharacter.CreateProp(propData, colorId);
        }
        #endregion Head_View

        #region Top_View
        void OnTopChanged(PropData propData, int colorId)
        {
            _equipments.Add(new EquipmentData(propData.id, colorId, propData.type));
            _tfCharacter.CreateProp(propData);
        }
        #endregion Top_View

        #region Bottom_View
        void OnBottomChanged(PropData propData, int colorId)
        {
            _equipments.Add(new EquipmentData(propData.id, colorId, propData.type));
            _tfCharacter.CreateProp(propData);
            Debug.Log($"[Customize] On Bottom Changed, PropName: {propData.name}");
        }
        #endregion Bottom_View

        #region Shoe_View
        void OnShoeChanged(PropData propData, int colorId)
        {
            _equipments.Add(new EquipmentData(propData.id, colorId, propData.type));
            _tfCharacter.CreateProp(propData);
        }
        #endregion Shoe_View

        #region Access_View
        void OnAccessChanged(PropData propData, int colorId)
        {
            _equipments.Add(new EquipmentData(propData.id, colorId, propData.type));
            _tfCharacter.CreateProp(propData);
        }
        #endregion Access_View

        #region Rotate_Charactor
        public void onDrag(BaseEventData eventData)
        {
            _tfCharacter.OnDrag();
        }
        public void onBeginDrag(BaseEventData eventData)
        {
            _tfCharacter.OnBeginDrag();
        }

        public void onEndDrag(BaseEventData eventData)
        {
            _tfCharacter.EndDrag();
        }
        #endregion Rotate_Charactor
    }
}