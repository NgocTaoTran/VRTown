using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using Nakama;
using Newtonsoft.Json;
using UnityEngine;
using VRTown.Game.Data;
using VRTown.Model;
using VRTown.Network;
using VRTown.Utilities;
using Zenject;

namespace VRTown.Game
{
    public interface ICharacterListener
    {
        void OnTransformChanged(Vector3 position, Vector3 eulerAngle, StateData stateData);
    }

    public partial class GameController : IGameController, ICharacterListener, System.IDisposable
    {
        public void OnAccountLoaded(IApiAccount account)
        {
        }

        public async void OnSocketConnected(UserMetaData userInfo)
        {
            var characterPosition = GameUtils.GetPositonByArray(userInfo.position);
            if (_userManager.ForceUpdate)
            {
                var userData = new UserMetaData();
                // userData.user_data.type = Model.CharacterType.User;
                userData.position = GameUtils.ConvertVector3ToArray(Vector3.zero);
                userData.user_data = _userManager.Profile;
                userInfo.user_data = _userManager.Profile;
                await _gameServer.UpdateAccount(userData);
                // await _gameServer.OnSendMatchStateAsync(_gameServer.MatchID, (long)OpCodes.UpdateMetaData, JsonConvert.SerializeObject(userData));
            }

            CreateMainCharacter(userInfo);
            _worldController.LoadWorld(characterPosition);

            UpdateMainTransform(characterPosition);

            // LoadPlayers();

            _onInitComplete?.Invoke();
        }

        public void OnReceivedStreamPresence(IStreamPresenceEvent presenceEvent)
        {
            // LeaveMembers(presenceEvent.Leaves);
        }

        public void OnReceivedStreamState(IStreamState streamState)
        {
            // _logger.Log("[GAME] OnReceivedStreamState: " + JsonConvert.SerializeObject(streamState.State));
            // switch ((StreamCodes)streamState.Stream.Mode)
            // {
            //     case StreamCodes.Join:
            //         var players = JsonConvert.DeserializeObject<List<UserMetaData>>(streamState.State);
            //         foreach (var player in players)
            //             CreatePlayer(player);
            //         break;
            // }
            // try
            // {
            //     var update = JsonConvert.DeserializeObject<Dictionary<string, object>>(streamState.State);
            //     Debug.Log("[OnReceivedStreamState]" + JsonConvert.SerializeObject(streamState));
            //     var stateData = JsonConvert.DeserializeObject<Data.StateData>(streamState.State);
            //     Debug.Log("[stateData]" + JsonConvert.SerializeObject(stateData.type));
            //     switch (stateData.type)
            //     {
            //         case StateType.moves:
            //             MoveMembers(stateData.members);
            //             break;
            //         case StateType.joins:
            //             JoinMembers(stateData.members);
            //             break;
            //         case StateType.changeSkin:
            //             UpdateSkinUser(stateData.members);
            //             break;
            //     }
            // }
            // catch (Exception e)
            // {
            //     Debug.LogError("exception " + e.Message);
            // // }
        }

        public void OnReceivedMatchState(IMatchState matchState)
        {
            // _logger.Log($"[GAME] OnReceivedMatchState, [OPCODE:{matchState.OpCode}] - [DATA: {Encoding.UTF8.GetString(matchState.State)}");
            switch ((OpCodes)matchState.OpCode)
            {
                case OpCodes.Join:
                    try
                    {
                        var userData = JsonConvert.DeserializeObject<List<UserMetaData>>(Encoding.UTF8.GetString(matchState.State));
                        // _logger.Log("[GAME] OnReceivedMatchState_Join: " + JsonConvert.SerializeObject(userData));
                        CreatePlayer(userData);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("exception " + e.Message);
                    }
                    break;

                case OpCodes.Move:
                    try
                    {
                        // _logger.Log("[GAME] OnReceivedMatchState_Move: " + Encoding.UTF8.GetString(matchState.State));
                        var characterData = JsonConvert.DeserializeObject<List<CharacterData>>(Encoding.UTF8.GetString(matchState.State));
                        // _logger.Log("[GAME] OnReceivedMatchState_Move: " + JsonConvert.SerializeObject(characterData));
                        MoveMembers(characterData);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("exception " + e.Message);
                    }
                    break;

                case OpCodes.UpdateMetaData:
                    try
                    {
                        // _logger.Log("[GAME] OnReceivedMatchState_UpdateMetaData: " + Encoding.UTF8.GetString(matchState.State));
                        var characterData = JsonConvert.DeserializeObject<List<CharacterData>>(Encoding.UTF8.GetString(matchState.State));
                        // _logger.Log("[GAME] OnReceivedMatchState_UpdateMetaData: " + JsonConvert.SerializeObject(characterData));
                        UpdateSkinUser(characterData);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("exception " + e.Message);
                    }
                    break;
                case OpCodes.Leave:
                    try
                    {
                        _logger.Log("[GAME] OnReceivedMatchState_UpdateMetaData: " + Encoding.UTF8.GetString(matchState.State));
                        var characterData = JsonConvert.DeserializeObject<List<UserLeaveMatch>>(Encoding.UTF8.GetString(matchState.State));
                        // _logger.Log("[GAME] OnReceivedMatchState_UpdateMetaData: " + JsonConvert.SerializeObject(characterData));
                        LeaveMembers(characterData);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("exception " + e.Message);
                    }
                    break;
            }
        }

        #region IUserCharacterListener
        public void OnTransformChanged(Vector3 position, Vector3 eulerAngle, StateData stateData)
        {
            var moveData = new CharacterData();
            moveData.x = position.x;
            moveData.y = position.z;
            moveData.z = position.y;
            moveData.d = eulerAngle.z;
            moveData.a = stateData;
            UpdateMainTransform(position);
            _gameServer.OnSendMatchStateAsync(_gameServer.MatchID, (long)OpCodes.Move, JsonConvert.SerializeObject(moveData));
            // _logger.Log("[GAME] OnTransformChanged_MoveData: " + JsonConvert.SerializeObject(moveData));

            var rect = new Rect(position.x - C.GameConfig.SizeLand, position.z - C.GameConfig.SizeLand, C.GameConfig.SizeLand * 2, C.GameConfig.SizeLand * 2);
            _worldController.LoadWorld(position);
        }
        #endregion IUserCharacterListener

        public void Dispose()
        {
            _gameServer.LeaveMatch();
        }

        void UpdateMainTransform(Vector3 position)
        {
            UpdateMiniMap(position);
            // CheckNearbyStream();
        }
    }
}