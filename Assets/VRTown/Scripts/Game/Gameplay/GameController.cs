using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTown.Network;
using Zenject;
using VRTown.Model;
using VRTown.Service;
using Newtonsoft.Json;
using Cysharp.Threading.Tasks;
using System.IO.Compression;
using System.IO;
using Nakama;
using UnityEngine.Pool;
using VRTown.Game.UI;
using System;

namespace VRTown.Game
{
    public interface IGameController : IServerListener
    {
        UniTask Initialize();
        void UpdateMainCharacter();
        void Setup(System.Action onEndMatch);
        void HideUI();

        void TapEnter(bool enable);
        void ForceHome();
        void OnSendMessage();
        void OnMouse();

        void EnableVoiceUserUI(uint uid, bool enabled);
        void EnableAudienceUserUI(uint uid, bool enabled);
    }

    public partial class GameController : IGameController
    {
        #region Zenject_Binding
        [Inject] VRTown.Service.ILogger _logger;
        [Inject(Id = BundleLoaderName.Resource)] readonly IBundleLoader _resourceLoader;
        [Inject(Id = BundleLoaderName.Zip)] readonly IBundleLoader _zipLoader;
        [Inject] readonly InputConfig _inputConfig;
        [Inject] readonly IModelController _modelController;
        [Inject] readonly IWorldController _worldController;
        [Inject] IGameServer _gameServer;
        [Inject] readonly IUserManager _userManager;
        [Inject] readonly Character.Factory _chaFactory;
        #endregion Zenject_Binding

        #region Local
        public GGameplay GGameplay;
        Character _mainCharacter;
        Dictionary<string, Character> _characters = new Dictionary<string, Character>();
        System.Action _onInitComplete = null;
        #endregion Local

        public async UniTask Initialize()
        {
            GHelper.Signal = _signalBus;
            _logger.Log("[GameController] Initialize");
            GGameplay = GameObject.Instantiate<GameObject>(_resourceLoader.LoadAssetLocal<GameObject>("Gameplay")).GetComponent<GGameplay>();
            GGameplay.Setup(this);

            await _modelController.Initialize();
            await _worldController.Initialize();
            RegisterSignals();
        }

        public void CreateMainCharacter(UserMetaData characterData)
        {
            _mainCharacter = _chaFactory.Create(CharacterType.User, characterData);
            _mainCharacter.SetCamera(GGameplay.GCamera);
            _mainCharacter.SetPlayController(GGameplay.Controller);
            _mainCharacter.SetListener(this);
            _characters.Add(characterData.user_data.id, _mainCharacter);
        }

        public void UpdateMainCharacter()
        {
            _mainCharacter.UpdateProfile(_userManager.Profile);

            var chaPos = C.MapConfig.ConvertMapPosition(_mainCharacter.Position);
            var charaterData = new CharacterData();
            charaterData.c = _userManager.Profile;
            // _gameServer.UpdateAccount(userData);
            _gameServer.OnSendMatchStateAsync(_gameServer.MatchID, (long)OpCodes.UpdateMetaData, JsonConvert.SerializeObject(charaterData));
            Debug.Log("[GAME]UpdateProfile:  " + JsonConvert.SerializeObject(charaterData));
        }

        public async void LoadPlayers()
        {
            //     var result = await _gameServer.RequestRPC(C.ServerConfig.NAME_RPC_LOAD_PLAYERS);
            //     _logger.Log("[NAKAMA] NAME_RPC_LOAD_PLAYERS: " + JsonConvert.SerializeObject(result));
            //     var players = JsonConvert.DeserializeObject<Dictionary<string, Member>>(result.Payload);
            //     _logger.Log("[NAKAMA] LIST_PLAYER: " + JsonConvert.SerializeObject(players));

            //     foreach (var player in players)
            //     {
            //         player.Value.id = player.Key;
            //         if (player.Key == _userManager.UserID || string.IsNullOrEmpty(player.Key)) continue;
            //         if (player.Key.Contains("bot"))
            //             CreateBot(player.Value);
            //         else
            //             CreatePlayer(player.Value);
            //     // }
        }

        public void CreateBot(UserMetaData botData)
        {
            // if (_characters.ContainsKey(botData.id))
            // {
            //     _logger.Error($"[CreateBot] ID = ({botData.id}) had exist!");
            //     return;
            // }
            // if (botData.name != "Train")
            // {
            //     var newPlayer = _chaFactory.Create(CharacterType.NPC, botData);
            //     _characters.Add(botData.id, newPlayer);
            // }

        }

        public void CreatePlayer(List<UserMetaData> userData)
        {
            foreach (var data in userData)
            {
                if (data == null) continue;
                if (_characters.ContainsKey(data.user_data.id))
                {
                    Debug.Log($"[GAME] CreatePlayer, Id = ({data.user_data.id}) had exist!");
                    return;
                }
                switch (data.user_data.type)
                {
                    case Model.CharacterType.Opponent:
                        var newPlayer = _chaFactory.Create((CharacterType.Opponent), data);
                        _characters.Add(data.user_data.id, newPlayer);
                        break;
                    case Model.CharacterType.NPC:
                        var newBot = _chaFactory.Create((CharacterType.NPC), data);
                        _characters.Add(data.user_data.id, newBot);
                        break;
                }
            }
        }

        // public void CreatePlayer(UserMData memberData)
        // {
        //     if (_characters.ContainsKey(memberData.id))
        //     {
        //         _logger.Error($"[CreatePlayer] ID = ({memberData.id}) had exist!");
        //         return;
        //     }

        //     var newPlayer = _chaFactory.Create(CharacterType.Opponent, memberData);
        //     _characters.Add(memberData.id, newPlayer);
        // }

        private void LeaveMembers(List<UserLeaveMatch> userData)
        {
            foreach (var data in userData)
            {
                if (_characters.ContainsKey(data.id))
                {
                    _characters[data.id].Release();
                    _characters.Remove(data.id);
                }
            }
        }

    }
}