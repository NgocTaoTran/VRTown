// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;
// using Agora.Rtc;
// using Cysharp.Threading.Tasks;
// using VRTown.Network;
// using VRTown.Game.UI;


// namespace VRTown.Game.UI
// {
//     public interface IVoiceChatController
//     {
//         void JoinVoiceChat();
//         void LeaveVoiceChat();
//     }
//     public class VoiceChatController : IVoiceChatController
//     {
//         private ArrayList permissionList = new ArrayList();

//         [SerializeField] private string _appID = "bea8a22fb49242689ddd648c0b149056";
//         [SerializeField] private string _token = "007eJxTYNi4doGQKP/UuurJCkcePNtx6KncVVkt+WOThNnat7A4XxZWYDCySE4yTTNINrNMSjJJSUtNMjVISTRIMTQyMDM3MDE1VIuQSWkIZGQQePuCgREKQXw2Bvec/KTEHAYGADpSHpA=";

//         [SerializeField] private string _channelName = "Global";
//         private IRtcEngine _rtcEngine;
//         private bool showVoiceList = true;
//         AgoraController _agoraController;

//         public void SetUp(AgoraController agoraController)
//         {
//             _agoraController = agoraController;
//         }
//         async void Update()
//         {
//             while (true)
//             {
//                 await CheckAction();
//             }

//         }

//         public async UniTask CheckAction()
//         {
//             await UniTask.NextFrame();
//         }

//         private void SetupVideoSDKEngine()
//         {
//             Debug.Log("[VoiceChat] SetupVideoSDKEngine");
//             _rtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngine();
//             RtcEngineContext context = new RtcEngineContext(_appID, 0, CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING, AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT, AREA_CODE.AREA_CODE_GLOB, null, "");
//             _rtcEngine.Initialize(context);

//         }

//         private void InitEventHandler()
//         {
//             Debug.Log("[VoiceChat] InitEventHandler");

//             UserEventHandler handler = new UserEventHandler(this);
//             _rtcEngine.InitEventHandler(handler);
//         }
//         // IEnumerator FetchVoiceList(uint uid)
//         // {
//         //     yield return new WaitForSeconds(0.5f);

//         //     GameClient.GetAgoraNakamaUserName(uid);
//         // }
//         public void JoinVoiceChat()
//         {
//             // Enables the audio module.
//             _rtcEngine.EnableAudio();
//             // Sets the user role ad broadcaster.
//             _rtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
//             // Joins a channel.
//             _rtcEngine.JoinChannel(_token, _channelName);
//         }
//         public void OnUserOfflineHandler(uint uid, USER_OFFLINE_REASON_TYPE reason, IGameServer gameServer)
//         {
//             // GameClient.UserLeaveVoiceList(uid);
//         }
//         public void OnUserJoinedHandler(uint uid, int elapsed)
//         {
//             string userJoinedMessage = string.Format("onUserJoined with uid {0}", uid);
//             Debug.Log(userJoinedMessage);
//             // StartCoroutine(FetchVoiceList(uid));
//         }
//         public void AdjustUserVolume(uint uid, int volume)
//         {
//             _rtcEngine.AdjustUserPlaybackSignalVolume(uid, volume);
//         }
//         public void AdjustLocalVolume(int volume)
//         {
//             _rtcEngine.AdjustPlaybackSignalVolume(volume);
//         }
//         public void LeaveVoiceChat()
//         {
//             if (_rtcEngine == null)
//                 return;
//             _rtcEngine.LeaveChannel();
//             // enableToggleVoicelist = false;
//             // GameClient.RemoveAllListUser();
//         }
//         private void MuteMic()
//         {
//             _rtcEngine.MuteLocalAudioStream(true);
//         }
//         private void UnmuteMic()
//         {
//             _rtcEngine.MuteLocalAudioStream(false);
//         }
//         public void OnErrorHandler(int error, string msg)
//         {
//             Debug.LogError(string.Format("Agora error err: {0}, msg: {1}", error, msg));
//         }
//         public void OnJoinChannelSuccessHandler(IGameServer gameServer)
//         {
//             // GameClient.JoinAgoraNakama(uid);
//             // Debug.Log("Agora joined channel " + channelName);
//             // joinButton.setActive(false);
//             // voiceButton.setActive(true);
//             // enableToggleVoicelist = true;
//         }
//         public void OnLeaveChannelHandler(RtcStats stats)
//         {
//             Debug.Log("Agora left channel at: " + stats.duration);


//         }
//     }
// }
