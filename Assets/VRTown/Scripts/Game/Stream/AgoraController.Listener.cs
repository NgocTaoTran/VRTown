using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using agora_gaming_rtc;
using VRTown.Model;

namespace VRTown.Game
{
    public partial class AgoraController
    {
        public void RegisterAgoraEvents()
        {
#if AGORA_ENABLE
            _rtcEngine.OnError += OnError;
            _rtcEngine.OnWarning += OnWarning;
            _rtcEngine.OnJoinChannelSuccess += OnJoinChannelSuccessHandler;
            _rtcEngine.OnLeaveChannel += OnLeaveChannelHandler;
            _rtcEngine.OnConnectionLost += OnConnectionLostHandler;
            _rtcEngine.OnUserJoined += OnUserJoinedHandler;
            _rtcEngine.OnUserOffline += OnUserOfflineHandler;
            _rtcEngine.OnClientRoleChanged += OnClientRoleChanged;
#endif
        }

        void OnError(int err, string msg)
        {
            _logger.Error($"[Agora] OnError, [ID:{err}] - [{msg}]");
        }

        void OnWarning(int warn, string msg)
        {
            _logger.Warning($"[Agora] OnWarning, [ID: {warn} - [{msg}]");
        }

        void OnJoinChannelSuccessHandler(string channelName, uint uid, int elapsed)
        {
            _logger.Log($"[Agora] OnJoinChannelSuccess, [ChannelID: {channelName}] - [GUID: {uid}] - [elapsed: {elapsed}]");
            foreach (var service in _agoraServices.Values)
                service.OnJoinChannelSuccess(channelName, uid, elapsed);
        }

        void OnLeaveChannelHandler(RtcStats stats)
        {
            _logger.Log($"[Agora] OnLeaveChannel, [Stats: {JsonConvert.SerializeObject(stats)}]");

            foreach (var service in _agoraServices)
            {
                _logger.Log("[Agora] service call leave" + service.Key);
                service.Value.OnLeaveChannel(stats);
            }

        }

        void OnUserJoinedHandler(uint uid, int elapsed)
        {
            _logger.Log($"[Agora] OnUserJoined, [GUID: {uid}] - [Elapsed: {elapsed}]");
#if AGORA_ENABLE
            foreach (var service in _agoraServices)
            {
                service.Value.OnUserJoined(uid, elapsed);
            }
#endif
            
        }

        void OnUserOfflineHandler(uint uid, USER_OFFLINE_REASON reason)
        {
            _logger.Log($"[Agora] OnUserOfflineHandler, [GUID: {uid}] - [Reson: {reason.ToString()}]");
#if AGORA_ENABLE
            foreach (var service in _agoraServices)
            {
                service.Value.OnUserOffline(uid, reason);
            }
#endif
            
        }

        void OnClientRoleChanged(CLIENT_ROLE_TYPE oldRole, CLIENT_ROLE_TYPE newRole)
        {
            _logger.Log($"[Agora] OnClientRoleChanged, [OldRole: {oldRole.ToString()}] - [NewRole: {newRole.ToString()}]");
            foreach (var service in _agoraServices.Values)
                service.OnRoleChanged(oldRole, newRole);
        }

        void OnConnectionLostHandler()
        {
            _logger.Log($"[Agora] OnConnectionLostHandler");
        }
    }
}