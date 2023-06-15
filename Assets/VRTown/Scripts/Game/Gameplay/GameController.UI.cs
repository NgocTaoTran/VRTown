using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTown.Network;
using Zenject;
using VRTown.Model;
using VRTown.Service;
using System.IO;
using VRTown.Game.UI;
using VRTown.Scene;
using UnityEngine.InputSystem;

namespace VRTown.Game
{
    public partial class GameController : IPlayViewListener
    {
        PlayUI _playUI;
        System.Action _onEndMatch;

        public void Setup(System.Action onEndMatch)
        {
            ShowUI();
            _onEndMatch = onEndMatch;
        }

        public void ShowUI()
        {
            _playUI = UIManager.Instance.ShowUIOnTop<PlayUI>("PlayUI");
            _playUI.Setup(_currentChannel, this, this);//, onPlay, onVoiceList, onJoin, onHost, onLeave, onJoinVoiceChat);
        }

        public void HideUI()
        {
            UIManager.Instance.ReleaseUI(_playUI, true);
        }

        public void TapEnter(bool enable)
        {
            _playUI.EnableChat(enable);
        }

        public void OnSendMessage()
        {
            Debug.Log("[NTAO] OnSendMessage");
            _playUI.UIChatView.OnTouchedSent();
        }

        void UpdateMiniMap(Vector3 position)
        {
            _playUI?.UIMiniMap?.OnTransformChanged(position);
        }

        public void TouchSetting()
        {
            InputSystem.DisableDevice(Keyboard.current);
            var settingUI = UIManager.Instance.ShowUIOnTop<SettingUI>("SettingUI");
            settingUI.Setup(GHelper.Localization.Language, (newLanguage) =>
            {
                GHelper.Localization.Language = newLanguage;
            }, () =>
            {
                InputSystem.EnableDevice(Keyboard.current);
            });
        }

        public void TouchInventory()
        {
            var inventoryUI = UIManager.Instance.ShowUIOnTop<CharacterCustomizeUI>("CharacterCustomizeUI");
            inventoryUI.Setup(_userManager.Profile, this, _modelController,
            (userData) =>
            {
                UIManager.Instance.ReleaseUI(inventoryUI, true);
                _userManager.Profile = userData;
                UpdateMainCharacter();
            });
        }

        public void TouchBackHome()
        {
            _onEndMatch?.Invoke();
        }

        public void onEnableListAudience(bool enable)
        {
            _playUI.UIAudiences.gameObject.SetActive(enable);
            _playUI.UIAudiences.Setup(onShareMic);
        }

        public void ToucheVoiceList()
        {
            Debug.Log("ToucheVoiceList");
        }
    }
}