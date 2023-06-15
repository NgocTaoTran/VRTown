using System.Collections;
using System.Collections.Generic;
using agora_gaming_rtc;
using UnityEngine;
using VRTown.Model;

namespace VRTown.Game
{
    public enum AgoraServiceState
    {
        Inactive,
        Active
    }
    
    public interface IAgoraService
    {
        public AgoraServiceState State { get; }
        AgoraService Type { get; }
        void RegisterOutput(object outputData);
        void RemoveOutput(object outputData);
        void Join(Dictionary<string, object> data);
        void Leave();
        object GetNearbySystem(Vector3 pos);

        // // Events Handler
        void OnJoinChannelSuccess(string channelName, uint uid, int elapsed);
        void OnLeaveChannel(RtcStats stats);
        void OnUserJoined(uint uid, int elapsed);
        void OnUserOffline(uint uid, USER_OFFLINE_REASON reason);
        void OnRoleChanged(CLIENT_ROLE_TYPE oldRole, CLIENT_ROLE_TYPE newRole);
    }
}