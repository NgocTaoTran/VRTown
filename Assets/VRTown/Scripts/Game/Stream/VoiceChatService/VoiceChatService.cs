using System.Collections;
using System.Collections.Generic;
using agora_gaming_rtc;
using UnityEngine;
using VRTown.Model;
using Cysharp.Threading.Tasks;

namespace VRTown.Game
{
    public class VoiceChatService : IAgoraService
    {
        public AgoraServiceState State { get { return _state; } }
        public AgoraService Type { get { return AgoraService.Streaming; } }

        List<AgoraServiceData> _servicesData = new List<AgoraServiceData>();
        AgoraServiceState _state;
        IAgoraController _controller;

        public VoiceChatService(IAgoraController controller, List<AgoraServiceData> data)
        {
            _controller = controller;
            _servicesData = data;
            _state = AgoraServiceState.Inactive;
        }

        public void RegisterOutput(object outputData)
        {
        }

        public void RemoveOutput(object outputData)
        {
        }

        public void Join(Dictionary<string, object> data)
        {
            var channelID = data[C.KeyConfigs.KEY_CHANNEL_ID] as string;
            var serviceData = _servicesData.Find(val => val.ID == channelID);
#if AGORA_ENABLE
            if (serviceData != null)
            {
                _controller.Engine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
                _controller.Engine.EnableAudio();
                _state = AgoraServiceState.Active;
                _controller.Engine.JoinChannelByKey(serviceData.Token, serviceData.Channel);
            }
#endif
        }

        public void Leave()
        {
#if AGORA_ENABLE
            if (_state == AgoraServiceState.Active)
            {
                _controller.Engine.DisableAudio();
                _controller.Engine.LeaveChannel();
            }
#endif
        }

        public object GetNearbySystem(Vector3 pos)
        {
            return null;
        }

        #region Events_Handler
        public void OnJoinChannelSuccess(string channelName, uint uid, int elapsed)
        {
            Debug.Log("[Agora] Voice Chat join chanel success");
            if (_state == AgoraServiceState.Active)
            {
                _controller.RegisterUserJoinVoiceChatAgora(uid);
            }
        }

        public void OnLeaveChannel(RtcStats stats)
        {
            // _streamView.Hide();
            _state = AgoraServiceState.Inactive;
        }

        public void OnUserJoined(uint uid, int elapsed)
        {
            Debug.Log("[Agora] Voice Chat User Joined");
            if (_state == AgoraServiceState.Active)
            {
                _controller.EnableVoiceUserUI(uid, true);
            }
        }

        public void OnUserOffline(uint uid, USER_OFFLINE_REASON reason)
        {
            Debug.Log("[Agora] Voice Chat User Leave");

            if (_state == AgoraServiceState.Active)
            {
                _controller.EnableVoiceUserUI(uid, false);
            }
        }

        public void OnRoleChanged(CLIENT_ROLE_TYPE oldRole, CLIENT_ROLE_TYPE newRole)
        {
            if (newRole == CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER)
            {
                // _streamView.SetupView(0);
            }
        }
        #endregion Events_Handler
    }
}