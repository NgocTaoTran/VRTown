using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Newtonsoft.Json;
using System;
using VRTown.Model;
using VRTown.Scene;
using VRTown.Game.UI;
using static VRTown.Model.C;

namespace VRTown.Game
{
    public partial class GameController : IGameController
    {
        public void ForceHome()
        {
            TeleportCharacter(new Vector3Int(0, 0, 0));
        }

        private void JoinMembers(List<CharacterData> members)
        {
            // foreach (var member in members)
            // {
            //     if (!_characters.ContainsKey(member.id) && member.id != _userManager.UserID)
            //     {
            //         CreatePlayer(JsonConvert.DeserializeObject<Member>(JsonConvert.SerializeObject(member)));
            //     }
            // }
        }

        private void UpdateSkinUser(List<CharacterData> members)
        {
            foreach (var member in members)
            {
                if (!_characters.ContainsKey(member.id))
                {
                    _logger.Error($"[CreatePlayer] ID = ({member.id}) had exist!");
                    return;
                }

                _characters[member.id].UpdateProfile(member.c);
            }
        }

        void MoveMembers(List<CharacterData> members)
        {
            foreach (var member in members)
            {
                if (!_characters.ContainsKey(member.id) || member.id == _userManager.UserID)
                {
                    return;
                }

                var deltaPos = Vector3.Distance(_characters[member.id].transform.position, new Vector3(member.x, member.z, member.y));
                if (deltaPos > 200)
                {
                    _characters[member.id].Teleport(member.x, member.y, member.z, member.d, member.gx, member.gy);
                }
                else
                {
                    _characters[member.id].Move(member.x, member.y, member.z, member.d, member.gx, member.gy);
                    _characters[member.id].Action(member.a);
                    Debug.Log("[Character_Move]: " + JsonConvert.SerializeObject(member.name) + ":" + member.z);
                }
            }
        }

        void TeleportCharacter(Vector3Int teleportPos)
        {
            _mainCharacter.Teleport(C.MapConfig.ConvertWorldPosition(teleportPos));
        }

        bool IsNearbyUser(string playerId)
        {
            if (_characters.ContainsKey(playerId))
                return Vector3.Distance(_characters[playerId].Position, _mainCharacter.Position) < C.CommunicationConfig.DistanceChatNearby;
            return false;
        }

        public void OnMouse()
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                var ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                Debug.DrawRay(ray.origin, ray.direction, Color.red);
                if (Physics.Raycast(ray, out RaycastHit hit, 10f))
                {
                    switch (hit.collider.tag)
                    {
                        case TagConfig.Screen:
                            var videoScreen = hit.collider.gameObject.GetComponent<GVideoScreen>();
                            if (videoScreen == null) return;
                            if (!videoScreen.IsPlay)
                            {
                                videoScreen.PlayVideo();
                                videoScreen.IsPlay = true;
                            }
                            else
                            {
                                videoScreen.PauseVideo();
                                videoScreen.IsPlay = false;
                            }
                            break;
                        case TagConfig.LiveScreen:
                            var liveScreen = hit.collider.gameObject.GetComponent<GLiveScreen>();
                            _agoraController.RegisterOutput(AgoraService.Streaming, liveScreen);
                            _playUI.UIStream.Setup(liveScreen.ID, liveScreen.Password, onHost, onAudience, onLeave, onEnableListAudience);
                            _playUI.UIStream.Show();
                            break;
                        case TagConfig.Door:
                            var controlDoor = hit.collider.gameObject.GetComponent<opencloseDoor>();
                            _playUI.UIPassword.Setup(controlDoor, onOpenDoor);
                            _playUI.UIPassword.Show();
                            break;
                        case TagConfig.AirBall:
                            Debug.Log("Touch Air Ball");
                            TeleportCharacter(new Vector3Int(104, 104));
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        void onOpenDoor(opencloseDoor controlDoor)
        {
            controlDoor.Open();
        }
    }
}