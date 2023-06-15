using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VRTown.Game.UI;
using VRTown.Model;
using VRTown.Network;
using VRTown.Scene;
using VRTown.Service;
using Zenject;
using static VRTown.Scene.GSMachine;

namespace VRTown.Game
{
    public partial class GamePlay : BaseState, IGamePlay
    {
        #region Zenject_Binding
        [Inject] VRTown.Service.ILogger _logger;
        [Inject] IGameController _gameController;
        [Inject] IModelController _modelController;
        [Inject] IUserManager _userManager;
        [Inject] IGameServer _gameServer;
        #endregion Zenject_Binding

        public override void OnStateEvent(StateEvent stateEvent)
        {
            if (stateEvent == StateEvent.Enter)
            {
                _logger.Log("GamePlay: State Enter");
                _gameController.Setup(onBackHome);
            }
            else if (stateEvent == StateEvent.Exit)
            {
                _gameController.HideUI();
                _gameServer.LeaveMatch();
                _logger.Log("GamePlay: State Exit");
            }
        }

        public void onSetting()
        {
            _logger.Log("Open Setting UI");
        }

        public void onInventery()
        {
            // Chinh cho player truoc khi dung vao Inventory View
            _logger.Log("Open Setting UI");
            var inventoryUI = UIManager.Instance.ShowUIOnTop<CharacterCustomizeUI>("CharacterCustomizeUI");
            inventoryUI.Setup(_userManager.Profile, _gameController, _modelController,
            (userData) =>
            {
                UIManager.Instance.ReleaseUI(inventoryUI, true);
                _userManager.Profile = userData;
                _gameController.UpdateMainCharacter();
            });
        }
        public void onBackHome()
        {
            GameFlow.Instance.SceneTransition(() =>
            {
                _gsMachine.ChangeState(GameState.GameHome);
            });
        }
    }
}