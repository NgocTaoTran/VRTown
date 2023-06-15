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

    public class UIAudience : MonoBehaviour
    {
        public AgoraService Type { get { return AgoraService.VoiceChat; } }
        [SerializeField] Scrmizu.InfiniteScrollRect _scrollUser;
        System.Action<uint, bool> _onShareMic;

        public void Setup(System.Action<uint, bool> onShareMic)
        {
            _onShareMic = onShareMic;
        }

        public void SetData(List<AgoraStreamUserData> data)
        {
            Debug.Log("[AgoraStreamUserData]" + Newtonsoft.Json.JsonConvert.SerializeObject(data));
            var convertData = new List<UIAudienceItemData>();
            foreach (var item in data)
            {
                convertData.Add(new UIAudienceItemData(item.uid, item.userName, OnShareMic));
            }
            _scrollUser.SetItemData(convertData);
        }

        public void OnShareMic(uint uid, bool enabled)
        {
            Debug.Log($"[Agora] Share Mic to [UID: {uid}] with [Enabled: {enabled}]");
            _onShareMic?.Invoke(uid, enabled);
        }
    }
}
