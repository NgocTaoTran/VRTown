using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRTown.Scene;
using VRTown.Model;
using DG.Tweening;

namespace VRTown.Game.UI
{
    public interface IPlayViewListener
    {
        void TouchSetting();
        void TouchInventory();
        void TouchBackHome();
        void EnableVoiceChat(bool enable);
        void SendMessage(ChatChannel channel, string message);
        void SwitchChannel(ChatChannel newChannel);
    }

    public class PlayUI : UIController, IChatListener
    {
        [SerializeField] public UIChatView UIChatView;
        [SerializeField] public UIMiniMap UIMiniMap;
        [SerializeField] public UIStreamView UIStream;
        [SerializeField] public UIPassword UIPassword;
        [SerializeField] public UIVoiceChat UIVoiceChat;
        [SerializeField] public UIAudience UIAudiences;

        [Header("Button")]
        [SerializeField] public GameObject JoinVoiceChatOn;
        [SerializeField] public GameObject JoinVoiceChatOff;

        IPlayViewListener _listener = null;
        IGameController _controller = null;
        bool _isInit = false;

        public void Setup(ChatChannel channel, IGameController controller, IPlayViewListener listener)
        {
            _listener = listener;
            _controller = controller;
            UIMiniMap.Setup();
            UIChatView.Setup(channel, this);
        }

        public void EnableChat(bool enabled)
        {
            GHelper.GameInput.SwitchCurrentActionMap(enabled ? C.InputConfig.ChatMap : C.InputConfig.NormalMap);
            UIChatView.gameObject.SetActive(enabled);
            UIChatView.UpdateUIChat(enabled);
            UIChatView.ActivateInputField();
        }

        public void UpdateUIVoiceChannel(string channelID, bool isDefault)
        {
            JoinVoiceChatOn.SetActive(true);
            JoinVoiceChatOff.SetActive(false);
            if (isDefault)
            {
                JoinVoiceChatOn.GetComponentInChildren<TextMeshProUGUI>().text = string.Format(GHelper.Localization.Localize<string>("TXT_JOIN_VOICE_CHAT_BIG"));
                JoinVoiceChatOn.GetComponentInChildren<LocalizeText>().KeyLocalization = "TXT_JOIN_VOICE_CHAT_BIG";
            }
            else
            {
                JoinVoiceChatOn.GetComponentInChildren<TextMeshProUGUI>().text = string.Format(GHelper.Localization.Localize<string>("TXT_JOIN_SUNLOFT_VOICE_CHAT_BIG"));
                JoinVoiceChatOn.GetComponentInChildren<LocalizeText>().KeyLocalization = "TXT_JOIN_SUNLOFT_VOICE_CHAT_BIG";
            }
        }

        public void TouchedSetting()
        {
            _listener?.TouchSetting();
        }
        public void TouchedBackHome()
        {
            _listener?.TouchBackHome();
        }

        public void TouchedInventory()
        {
            _listener?.TouchInventory();
        }

        #region IChatListener
        public void SwitchChanel(ChatChannel channel)
        {
            _listener?.SwitchChannel(channel);
        }

        public void ChatMessage(ChatChannel channel, string message)
        {
            _listener?.SendMessage(channel, message);
        }
        #endregion IChatListener


        #region IVoiceListener
        public void TouchedVoice(bool enabled)
        {
            _listener?.EnableVoiceChat(enabled);
        }
        #endregion IVoiceListener
    }
}