using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTown.Model;
using agora_gaming_rtc;

namespace VRTown.Game
{
    public class StreamService : IAgoraService
    {
        public AgoraServiceState State { get { return _state; } }
        public AgoraService Type { get { return AgoraService.Streaming; } }
        Dictionary<string, GLiveScreen> _screens = new Dictionary<string, GLiveScreen>();

        List<AgoraServiceData> _servicesData = new List<AgoraServiceData>();
        AgoraServiceState _state;
        IAgoraController _controller;
        GLiveScreen _streamView;

        string _currentScreen = "";
        bool _isHost = false;

        public StreamService(IAgoraController controller, List<AgoraServiceData> data)
        {
            _controller = controller;
            _servicesData = data;
            _state = AgoraServiceState.Inactive;
        }

        public void ConfigStream(GLiveScreen screenInfo)
        {
            VideoEncoderConfiguration config = new VideoEncoderConfiguration();
            config.dimensions.width = screenInfo.Width;
            Debug.Log("[screenInfo] Width: " + config.dimensions.width);
            config.dimensions.height = screenInfo.Height;
            Debug.Log("[screenInfo] Height: " + config.dimensions.height);
            config.bitrate = screenInfo.Bitrate;
            config.orientationMode = (ORIENTATION_MODE)screenInfo.Orientation;
            config.degradationPreference = DEGRADATION_PREFERENCE.MAINTAIN_QUALITY;
#if AGORA_ENABLE
            _controller.Engine.SetVideoEncoderConfiguration(config);
#endif
        }

        public void RegisterOutput(object outputData)
        {
            if (outputData.GetType() == typeof(GLiveScreen))
            {
                var liveScreen = outputData as GLiveScreen;
                if (!_screens.ContainsKey(liveScreen.ID))
                {
                    _screens.Add(liveScreen.ID, liveScreen);
                }
            }
        }

        public void RemoveOutput(object outputData)
        {
            if (outputData.GetType() == typeof(GLiveScreen))
            {
                var liveScreen = outputData as GLiveScreen;
                if (_screens.ContainsKey(liveScreen.ID))
                {
                    _screens.Remove(liveScreen.ID);
                }
            }
        }

        public void Join(Dictionary<string, object> data)
        {
            _isHost = (bool)data[C.KeyConfigs.KEY_HOST];
            var screenId = data[C.KeyConfigs.KEY_CHANNEL_ID] as string;
            Debug.LogError($"[KEY_CHANNEL_ID] ScreenID [{screenId}]");
            if (!_screens.ContainsKey(screenId))
            {
                Debug.LogError($"[StreamService] ScreenID [{screenId}] doesn't EXIST!");
                return;
            }

            _state = AgoraServiceState.Active;
            _currentScreen = screenId;
            _streamView = _screens[_currentScreen];
            ConfigStream(_screens[screenId]);
            var serviceData = _servicesData.Find(val => val.ID == screenId);
#if AGORA_ENABLE
            if (serviceData != null)
            {
                _controller.Engine.SetClientRole(_isHost ? CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER : CLIENT_ROLE_TYPE.CLIENT_ROLE_AUDIENCE);
                if (_isHost)
                {
                    Debug.Log("[Agora] Setup agora");
                    _controller.Engine.EnableVideo();
                    _controller.Engine.EnableAudio();
                    _controller.Engine.EnableVideoObserver();
                    _streamView.SetupView(0);
                    _streamView.Show();
                }
                _controller.Engine.JoinChannelByKey(serviceData.Token, serviceData.Channel);
            }
#endif
        }

        public void Leave()
        {
#if AGORA_ENABLE
            _controller.Engine.DisableVideo();
            _controller.Engine.DisableAudio();
            _controller.Engine.DisableVideoObserver();
            _controller.Engine.LeaveChannel();
#endif
        }

        public object GetNearbySystem(Vector3 pos)
        {
            foreach (var screen in _screens)
            {
                var distance = Vector3.Distance(pos, screen.Value.transform.position);
                if (distance < C.CommunicationConfig.DistanceScreenNearby)
                    return screen.Value as GLiveScreen;
            }
            return null;
        }

        #region Events_Handler
        public void OnJoinChannelSuccess(string channelName, uint uid, int elapsed)
        {
            if (_state == AgoraServiceState.Active)
            {
                _controller.RegisterUserJoinStreamAgora(uid);
                _controller.EnableAudienceUserUI(uid, true);
            }

        }

        public void OnLeaveChannel(RtcStats stats)
        {
            if (_state == AgoraServiceState.Active)
            {
                _streamView.Hide();
            }
        }

        public void OnUserJoined(uint uid, int elapsed)
        {
            if (_state == AgoraServiceState.Active)
            {
                _streamView.SetupView(uid);
                _streamView.Show();
            }
        }

        public void OnUserOffline(uint uid, USER_OFFLINE_REASON reason)
        {
            if (_state == AgoraServiceState.Active)
            {
                _streamView.Hide();
                _controller.EnableAudienceUserUI(uid, false);
            }
        }

        public void OnRoleChanged(CLIENT_ROLE_TYPE oldRole, CLIENT_ROLE_TYPE newRole)
        {
            if (_state == AgoraServiceState.Active)
            {
                if (newRole == CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER)
                {
                    _streamView.SetupView(0);
                }
            }
        }
        #endregion Events_Handler


    }
}