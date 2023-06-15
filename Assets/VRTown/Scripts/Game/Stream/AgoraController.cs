using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.Serialization;
using VRTown.Network;
using Cysharp.Threading.Tasks;
using Nakama;
using VRTown.Model;
using Zenject;
using VRTown.Game.UI;
using System.Runtime.InteropServices;
using UnityEngine.Networking;
using VRTown.Service;
using Newtonsoft.Json;
#if AGORA_ENABLE
using agora_gaming_rtc;
#endif

namespace VRTown.Game
{
    public interface IAgoraController
    {
#if AGORA_ENABLE
        IRtcEngine Engine { get; }
#endif
        UniTask Initialize();
        void RegisterOutput(AgoraService service, object goOutput);
        void JoinService(AgoraService service, Dictionary<string, object> data);
        IAgoraService GetService(AgoraService service);
        void LeaveService(AgoraService service);
        void RegisterUserJoinVoiceChatAgora(uint uid);
        void RegisterUserJoinStreamAgora(uint uid);
        void EnableVoiceUserUI(uint uid, bool enabled);
        void EnableAudienceUserUI(uint uid, bool enabled);

        void MuteAll(int volume);
        void MutePlayer(uint uid, bool enabled);
    }

    public partial class AgoraController : IAgoraController, System.IDisposable
    {
        #region Zenject_Binding
        [Inject] readonly VRTown.Service.ILogger _logger;
        [Inject] readonly IApiManager _apiManager;
        [Inject] readonly IGameController _gamecontroller;
        [Inject] readonly IUserManager _userManager;
        [Inject] readonly IGameServer _gameServer;
        #endregion Zenject_Binding

        #region Local
        AgoraData _agoraConfig;

#if AGORA_ENABLE
        public IRtcEngine Engine { get { return _rtcEngine; } }
        IRtcEngine _rtcEngine = null;
#endif

        Dictionary<AgoraService, IAgoraService> _agoraServices = new Dictionary<AgoraService, IAgoraService>();
        #endregion Local

        public async UniTask Initialize()
        {
            Debug.Log("[Agora] Initialize");
            await LoadConfig();
            SetupEngine();
        }

        private async UniTask LoadConfig()
        {
            var (response, ex) = await _apiManager.GetAgoraConfiguration();
            if (ex != null)
            {
                _logger.Error("[Agora] Load Config failed, message = " + ex.Message);
                return;
            }
            _agoraConfig = response;
            _logger.Log("[Agora] Agora Config, " + Newtonsoft.Json.JsonConvert.SerializeObject(_agoraConfig));
        }

        private void SetupEngine()
        {
            _logger.Log("[Agora] GetEngine: " + _agoraConfig.AppID);

#if AGORA_ENABLE
            _rtcEngine = IRtcEngine.GetEngine(_agoraConfig.AppID);
            _rtcEngine.SetChannelProfile(CHANNEL_PROFILE.CHANNEL_PROFILE_LIVE_BROADCASTING);
#endif
            RegisterAgoraEvents();
        }

        public void RegisterOutput(AgoraService service, object goOutput)
        {
            GetService(service).RegisterOutput(goOutput);
        }

        public IAgoraService GetService(AgoraService service)
        {
            if (!_agoraServices.ContainsKey(service))
                _agoraServices.Add(service, CreateNewService(service));
            Debug.Log("[Create Agora service]" + Newtonsoft.Json.JsonConvert.SerializeObject(_agoraServices));
            return _agoraServices[service];
        }

        IAgoraService CreateNewService(AgoraService service)
        {
            switch (service)
            {
                case AgoraService.Streaming:
                    return new StreamService(this, _agoraConfig.Services.Where(val => val.Type == AgoraService.Streaming).ToList());

                case AgoraService.VoiceChat:
                    return new VoiceChatService(this, _agoraConfig.Services.Where(val => val.Type == AgoraService.VoiceChat).ToList());

                default:
                    return null;
            }
        }

        public void JoinService(AgoraService service, Dictionary<string, object> data)
        {
            GetService(service).Join(data);
        }

        public void LeaveService(AgoraService service)
        {
            GetService(service).Leave();
        }

        public void MuteAll(int volume)
        {
            Debug.Log("[Agora] MuteAll: " + volume);
#if AGORA_ENABLE
            _rtcEngine.AdjustPlaybackSignalVolume(volume);
#endif
        }

        public void MutePlayer(uint uid, bool enabled)
        {
            Debug.Log($"[Agora] MutePlayer {uid}: {enabled}");
#if AGORA_ENABLE
            _rtcEngine.MuteRemoteAudioStream(uid, !enabled);
#endif
        }

        public void Dispose()
        {
            _logger.Log("[Agora] Dispose!");
#if AGORA_ENABLE
            if (_rtcEngine != null)
            {
                IRtcEngine.Destroy();
            }
#endif
        }

        public async void RegisterUserJoinVoiceChatAgora(uint uid)
        {
            var payload = new Dictionary<string, object>
            {
                {"agoraId",uid },
                {"userName",_userManager.UserName }
            };
            var apiRpc = await _gameServer.RequestRPC(C.ServerConfig.NAME_RPC_REGISTER_JOIN_VOICE_AGORA, JsonConvert.SerializeObject(payload));
            _logger.Log("[NAKAMA] RegisterUserJoinVoiceChatAgora" + JsonConvert.SerializeObject(apiRpc));
        }
        public async void RegisterUserJoinStreamAgora(uint uid)
        {
            var payload = new Dictionary<string, object>
            {
                {"agoraId",uid},
                {"userName",_userManager.UserName}
            };
            var apiRpc = await _gameServer.RequestRPC(C.ServerConfig.NAME_RPC_REGISTER_JOIN_STREAM_AGORA, JsonConvert.SerializeObject(payload));
            _logger.Log("[NAKAMA] RegisterUserJoinVoiceStreamAgora" + JsonConvert.SerializeObject(apiRpc));
        }

        public void EnableVoiceUserUI(uint uid, bool enabled)
        {
            _logger.Log($"[Agora] Update ID for ID: {uid}!");
            _gamecontroller.EnableVoiceUserUI(uid, enabled);
        }

        public void EnableAudienceUserUI(uint uid, bool enabled)
        {
            _logger.Log($"[Agora] Update ID for ID: {uid}!");
            _gamecontroller.EnableAudienceUserUI(uid, enabled);
        }
    }
}