using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRTown.Model;
using UnityEngine.InputSystem;

namespace VRTown.Game.UI
{
    public interface IChatListener
    {
        public void SwitchChanel(ChatChannel channel);
        public void ChatMessage(ChatChannel channel, string message);
    }

    public class UIChatView : MonoBehaviour
    {
        [SerializeField] Scrmizu.InfiniteScrollRect _scroolMessage;
        [SerializeField] TMP_InputField _inputChat;
        [SerializeField] Button _btnSend;

        [Header("Channels")]
        [SerializeField] Toggle _toggleGlobal;
        [SerializeField] Toggle _toggleNearby;
        [SerializeField] Toggle _togglePrivate;
        [Header("Button")]
        [SerializeField] public GameObject JoinChatOn;
        [SerializeField] public GameObject JoinChatOff;

        ChatChannel _chatChannel = ChatChannel.Global;

        IChatListener _chatListener = null;

        public void Setup(ChatChannel channel, IChatListener listener)
        {
            _chatChannel = channel;
            _chatListener = listener;

            _toggleGlobal.SetIsOnWithoutNotify(_chatChannel == ChatChannel.Global);
            _toggleNearby.SetIsOnWithoutNotify(_chatChannel == ChatChannel.Nearby);
        }

        public void SetMessageData(IEnumerable<MessageData> message)
        {
            _scroolMessage.SetItemData(message);
        }

        public void ActivateInputField()
        {
            _inputChat.ActivateInputField();
        }

        public void SwitchChat(Toggle toggle)
        {
            if (toggle.isOn)
            {
                ChatChannel channel = ChatChannel.Global;
                System.Enum.TryParse<ChatChannel>(toggle.GetComponent<UIToggleID>().ID, out channel);
                _chatListener?.SwitchChanel(channel);
            }
        }

        public void OnInputChat()
        {
            _btnSend.interactable = _inputChat.text.Length > 0;

            var deviceKeyboard = InputSystem.GetDevice<Keyboard>();
            if (deviceKeyboard != null && deviceKeyboard.enterKey.isPressed && _inputChat.text != "\n")
            {
                OnTouchedSent();
            }
            else if (_inputChat.text == "\n")
            {
                GHelper.GameInput.SwitchCurrentActionMap(C.InputConfig.NormalMap);
                UpdateUIChat(false);
                this.gameObject.SetActive(false);
                _inputChat.text = null;
            }
        }

        public void OnInputEndEdit(string value)
        {
            var deviceKeyboard = InputSystem.GetDevice<Keyboard>();
            if (deviceKeyboard != null && deviceKeyboard.enterKey.isPressed)
            {
                OnTouchedSent();
            }
        }

        public void OnTouchedSent()
        {
            if (_inputChat.text.Length <= 0) return;
            _chatListener?.ChatMessage(_chatChannel, _inputChat.text);
            _inputChat.text = "";
        }

        public void UpdateUIChat(bool isActive)
        {
            JoinChatOn.SetActive(!isActive);
            JoinChatOff.SetActive(isActive);
        }
    }
}