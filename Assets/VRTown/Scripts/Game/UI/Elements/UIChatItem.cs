using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Scrmizu;
using VRTown.Model;
using Newtonsoft.Json;
using UnityEngine.UI;

namespace VRTown.Game.UI
{
    public class UIChatItem : MonoBehaviour, IInfiniteScrollItem
    {
        [SerializeField] VerticalLayoutGroup _layout;
        [SerializeField] Image _background;
        [SerializeField] TMP_Text _textName;
        [SerializeField] TMP_Text _textMessage;

        MessageData _data;
        public void Hide()
        {
            this.gameObject.SetActive(false);
        }

        public void UpdateItemData(object data)
        {
            if (!(data is MessageData)) return;
            _data = data as MessageData;

            gameObject.SetActive(true);

            var messageContent = JsonConvert.DeserializeObject<Dictionary<string, string>>(_data.Content);
            foreach (var item in messageContent)
            {
                _textName.text = item.Key;
                _textMessage.text = item.Value;
            }
            Debug.LogError("UserID: " + GHelper.UserNID);
            Layout(_data.SenderId == GHelper.UserNID);
        }

        void Layout(bool isUser)
        {
            _textName.alignment = isUser ? TextAlignmentOptions.Left : TextAlignmentOptions.Right;
            _textMessage.alignment = isUser ? TextAlignmentOptions.Left : TextAlignmentOptions.Right;
            _layout.childAlignment = isUser ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight;
            _background.color = isUser ? Color.blue : Color.gray;
        }
    }
}