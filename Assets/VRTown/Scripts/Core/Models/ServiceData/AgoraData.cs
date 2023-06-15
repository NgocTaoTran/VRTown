using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

namespace VRTown.Model
{
    public enum AgoraService
    {
        Streaming,
        VoiceChat,
        TextChat
    }

    public class AgoraServiceData
    {
        public string ID;
        [JsonConverter(typeof(StringEnumConverter))] public AgoraService Type;
        public string Channel;
        public string Token;
    }

    public class AgoraData
    {
        public string AppID = "28cb5f0c69bb4dfeb50da0d120670451";
        public string TempToken = "007eJxTYDi/4kLvodWvrOZuTIsXO7KEl3H9yw+/TlYbJu6s2bRySfYlBQYji+Qk0zSDZDPLpCSTlLTUJFODlESDFEMjAzNzAxNTw4mxhikNgYwMhmtmMzBCIYjPyxCSWlwSlp+ZnOqckVjCwAAAoyIlsQ==";
        public string ChannelVoiceChat = "TestVoiceChat";
        public string DefaultChannel = "Global";
        public AgoraServiceData[] Services;
    }

    public class AgoraVoiceUserData
    {
        public string userName;
        public uint uid;
    }
    public class AgoraStreamUserData
    {
        public string userName;
        public uint uid;
    }
}