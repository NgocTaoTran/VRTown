using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRTown.Scene;
using Nakama;
using System;
using VRTown.Service;
using VRTown.Model;

namespace VRTown.Game.UI
{

    public class UIVoiceChat : MonoBehaviour
    {
        public AgoraService Type { get { return AgoraService.VoiceChat; } }

        [SerializeField] Scrmizu.InfiniteScrollRect _scrollUser;

        System.Action<int> _onMuteAll;
        System.Action<uint, bool> _onMutePlayer;

        public void Setup(System.Action<int> onToucedMuteAll, System.Action<uint, bool> onMuteUser)
        {
            _onMuteAll = onToucedMuteAll;
            _onMutePlayer = onMuteUser;
        }

        public void SetData(List<AgoraVoiceUserData> data)
        {
            Debug.Log("[data]" + Newtonsoft.Json.JsonConvert.SerializeObject(data));
            var convertData = new List<UIVoiceItemData>();
            foreach (var item in data)
            {
                convertData.Add(new UIVoiceItemData(item.uid, item.userName, OnMute));
            }
            _scrollUser.SetItemData(convertData);
        }

        public void TouchedMuteAll(int volume)
        {
            _onMuteAll?.Invoke(volume);
        }

        public void OnMute(uint uid, bool enabled)
        {
            Debug.Log($"[Agora] User Mute [UID: {uid}] with [Enabled: {enabled}]");
            _onMutePlayer?.Invoke(uid, enabled);
        }
    }
}
