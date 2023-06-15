using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTown.Model;
using VRTown.Game.UI;
using VRTown.Utilities;
using Zenject;
using Newtonsoft.Json;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;

namespace VRTown.Game
{
    public partial class GameController : IPlayViewListener
    {
        #region Zenject_Binding
        [Inject] readonly IAgoraController _agoraController;
        [Inject] private SignalBus _signalBus;
        #endregion Zenject_Binding

        ChatChannel _currentChannel = ChatChannel.Global;
        PriorityList<MessageData> _messages = new PriorityList<MessageData>();
        List<AgoraVoiceUserData> _voiceChatUser = new List<AgoraVoiceUserData>();
        List<AgoraStreamUserData> _streamAudienceUser = new List<AgoraStreamUserData>();
        Dictionary<uint, string> _loadUserName = new Dictionary<uint, string>();
        GLiveScreen _nearbyScreen = null;
        string _currentVoiceChannel = "Voice_Default";

        public void RegisterSignals()
        {
            _signalBus.Subscribe<ForceVoiceSignal>(OnVoiceSignal);
        }

        #region Chat
        public void OnMessageChanged(PriorityList<MessageData> messages)
        {
            _messages = messages;
            _playUI.UIChatView.SetMessageData(FilterMessages(_currentChannel));
        }

        public void SwitchChannel(ChatChannel channel)
        {
            _currentChannel = channel;
            _playUI.UIChatView.SetMessageData(FilterMessages(_currentChannel));
        }

        IEnumerable<MessageData> FilterMessages(ChatChannel channel)
        {
            switch (channel)
            {
                case ChatChannel.Global:
                    {
                        return _messages;
                    }

                case ChatChannel.Nearby:
                    {
                        var messages = from message in _messages where (IsNearbyUser(message.SenderId) || message.SenderId == GHelper.UserNID) select message;
                        return messages;
                    }

                default:
                    return _messages;
            }
        }

        void ProcessCommand(string textCommand)
        {
            var pathComs = textCommand.Split(" ");

            string command = "";
            if (pathComs.Length > 0)
                command = pathComs[0];

            switch (command)
            {
                case "/goto":
                    {
                        var parameters = pathComs[1].Split(",");
                        Vector3Int _teleport = new Vector3Int(0, 0);
                        if (parameters.Length > 0)
                            _teleport.x = System.Convert.ToInt32(parameters[0].Trim());
                        if (parameters.Length > 1)
                            _teleport.y = System.Convert.ToInt32(parameters[1].Trim());
                        _teleport.z = 0;
                        TeleportCharacter(_teleport);
                        break;
                    }
            }
        }

        public void SendMessage(ChatChannel channel, string message)
        {
            message = message.Trim();
            if (message.StartsWith('/'))
            {
                ProcessCommand(message);
            }
            else
            {
                ProcessChat(channel, message);
            }
        }

        void ProcessChat(ChatChannel channel, string message)
        {

            _gameServer.SendMessage(channel, new MessageData(_userManager.UserName, message));
        }
        #endregion Chat

        #region Streaming
        // void CheckNearbyStream()
        // {
        //     var streamService = _agoraController.GetService(AgoraService.Streaming);
        //     if (_nearbyScreen == null)
        //     {
        //         _nearbyScreen = streamService.GetNearbySystem(_mainCharacter.Position) as GLiveScreen;
        //         if (_nearbyScreen != null)
        //         {
        //             _playUI.UIStream.Setup(_nearbyScreen.ID, _nearbyScreen.Password, onHost, onAudience, onLeave, onEnableListAudience);
        //             _playUI.UIStream.Show();
        //         }
        //     }
        //     else
        //     {
        //         var newScreen = streamService.GetNearbySystem(_mainCharacter.Position) as GLiveScreen;
        //         if (newScreen != _nearbyScreen)
        //         {
        //             _agoraController.LeaveService(AgoraService.Streaming);
        //             _playUI.UIStream.Hide();
        //             _nearbyScreen = newScreen;

        //             if (_nearbyScreen != null)
        //             {
        //                 _playUI.UIStream.Setup(_nearbyScreen.ID, _nearbyScreen.Password, onHost, onAudience, onLeave, onEnableListAudience);
        //                 _playUI.UIStream.Show();
        //             }
        //         }
        //     }
        // }

        void onHost(string screenId)
        {
            var data = new Dictionary<string, object>() {
                { C.KeyConfigs.KEY_HOST, true },
                // { C.KeyConfigs.KEY_CHANNEL_ID, _nearbyScreen.ID },
                { C.KeyConfigs.KEY_CHANNEL_ID, (screenId != null)? screenId: _nearbyScreen.ID}
            };
            _agoraController.JoinService(AgoraService.Streaming, data);

        }
        void onAudience(string screenId)
        {
            var data = new Dictionary<string, object>() {
                { C.KeyConfigs.KEY_HOST, false },
                { C.KeyConfigs.KEY_CHANNEL_ID, (screenId != null)? screenId: _nearbyScreen.ID},
            };
            _agoraController.JoinService(AgoraService.Streaming, data);
        }

        void onLeave(AgoraService type)
        {
            _agoraController.LeaveService(AgoraService.Streaming);
        }

        void onShareMic(uint uid, bool enabled)
        {
            Debug.Log("[Agora] Touched Share mic");
        }
        public async void EnableAudienceUserUI(uint uid, bool enabled)
        {
            _logger.Log($"[Agora] {enabled.ToString().ToUpperInvariant()} Audience User UI: {uid}");
            if (enabled)
            {
                var userData = await GetUserNameJoinStreamAgora<AgoraStreamUserData>(uid);
                userData.uid = uid;
                if (userData != null)
                    _streamAudienceUser.Add(userData);
                else
                    _logger.Error($"[Agora] Show UI for [UID: {uid}] has issue!");
            }
            else
            {
                var userData = _streamAudienceUser.Find(val => val.uid == uid);
                if (userData != null)
                    _streamAudienceUser.Remove(userData);
            }
            _playUI.UIAudiences.SetData(_streamAudienceUser);
        }
        #endregion Streaming

        #region Voice Chat
        public void EnableVoiceChat(bool enable)
        {
            var data = new Dictionary<string, object>() {
                { C.KeyConfigs.KEY_CHANNEL_ID, _currentVoiceChannel}
            };
            if (enable)
            {
                _voiceChatUser.Clear();
                _playUI.UIVoiceChat.Setup(OnMuteAll, OnUserMute);

                _agoraController.JoinService(AgoraService.VoiceChat, data);
            }
            else
            {
                _voiceChatUser.Clear();
                _agoraController.LeaveService(AgoraService.VoiceChat);
            }
        }

        public void OnMuteAll(int volume)
        {
            _agoraController.MuteAll(volume);
        }

        public void OnUserMute(uint uid, bool enabled)
        {
            _logger.Log($"[Agora] User Mute [UID: {uid}] with [Enabled: {enabled}]");
            _agoraController.MutePlayer(uid, enabled);
        }

        public async void EnableVoiceUserUI(uint uid, bool enabled)
        {
            _logger.Log($"[Agora] {enabled.ToString().ToUpperInvariant()} Voice User UI: {uid}");
            if (enabled)
            {
                var userData = await GetUserNameJoinVoiceAgora<AgoraVoiceUserData>(uid);
                userData.uid = uid;
                if (userData != null)
                    _voiceChatUser.Add(userData);
                else
                    _logger.Error($"[Agora] Show UI for [UID: {uid}] has issue!");
            }
            else
            {
                var userData = _voiceChatUser.Find(val => val.uid == uid);
                if (userData != null)
                    _voiceChatUser.Remove(userData);
            }
            _playUI.UIVoiceChat.SetData(_voiceChatUser);
        }
        #endregion Voice Chat

        async UniTask<AgoraVoiceUserData> GetUserNameJoinVoiceAgora<AgoraVoiceUserData>(uint uid)
        {
            var payload = new Dictionary<string, uint> { { "agoraId", uid } };
            var result = await _gameServer.RequestRPC(C.ServerConfig.NAME_RPC_GET_NAME_JOIN_VOICE_AGORA, JsonConvert.SerializeObject(payload));
            _logger.Log("[AGORA] User Joined Voice: " + JsonConvert.SerializeObject(result));
            if (!string.IsNullOrEmpty(result.Payload) && result.Payload != "null")
            {
                var user = JsonConvert.DeserializeObject<AgoraVoiceUserData>(result.Payload);
                return user;
            }
            return default;
        }
        async UniTask<AgoraStreamUserData> GetUserNameJoinStreamAgora<AgoraStreamUserData>(uint uid)
        {
            var payload = new Dictionary<string, uint> { { "agoraId", uid } };
            var result = await _gameServer.RequestRPC(C.ServerConfig.NAME_RPC_GET_NAME_JOIN_STREAM_AGORA, JsonConvert.SerializeObject(payload));
            _logger.Log("[AGORA] User Joined Stream: " + JsonConvert.SerializeObject(result));
            if (!string.IsNullOrEmpty(result.Payload) && result.Payload != "null")
            {
                var user = JsonConvert.DeserializeObject<AgoraStreamUserData>(result.Payload);
                return user;
            }
            return default;
        }

        public async void OnVoiceSignal(ForceVoiceSignal signal)
        {
            Debug.Log("[OnVoiceSignal] working");
            _currentVoiceChannel = signal.IsTrigerEnter ? signal.ChannelID : "Voice_Default";
            await WaitForPlayUILoaded();
            if (_agoraController.GetService(AgoraService.VoiceChat) != null)
                _agoraController.LeaveService(AgoraService.VoiceChat);
            _playUI.UIVoiceChat.gameObject.SetActive(false);
            _playUI.UpdateUIVoiceChannel(signal.ChannelID, !signal.IsTrigerEnter);
            if (_voiceChatUser.Count == 0) return;
            _voiceChatUser.Clear();
            _playUI.UIVoiceChat.SetData(_voiceChatUser);
        }

        private async UniTask WaitForPlayUILoaded()
        {
            while (_playUI == null || _playUI.UIVoiceChat == null)
            {
                await UniTask.Delay(1); // Wait for a short time
            }
        }
    }
}