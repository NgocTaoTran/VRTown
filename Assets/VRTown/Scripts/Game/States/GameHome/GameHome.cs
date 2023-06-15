using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
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
    public partial class GameHome : BaseState, IGameHome
    {
        #region Zenject_Binding
        [Inject] VRTown.Service.ILogger _logger;
        [Inject] IGameServer _gameServer;
        [Inject] IUserManager _userManager;
        [Inject] IGameController _gameController;
        [Inject] IAgoraController _agoraController;
        [Inject] IWalletController _walletController;

        #endregion Zenject_Binding

        #region Properties
        #endregion Properties

        #region Local
        HomeUI _homeUI;

        #endregion Local

        public override void OnStateEvent(StateEvent stateEvent)
        {
            if (stateEvent == StateEvent.Enter)
            {
                _homeUI = UIManager.Instance.ShowUIOnTop<HomeUI>("HomeUI");
                _homeUI.Setup(LoadMap);
                _agoraController.Initialize();
            }
            else if (stateEvent == StateEvent.Exit)
            {
                _logger.Log("[GAME] GameHome: State Exit");
                UIManager.Instance.ReleaseUI(_homeUI, true);
            }
        }

        public async void LoadMap(string mapName)
        {
            Debug.Log("[Find_MatchID]: " + mapName);
            await _gameServer.FindMatch(mapName, onConnected);
        }

        void onConnected()
        {
            GameFlow.Instance.SceneTransition(() =>
            {
                _gsMachine.ChangeState(GameState.Gameplay);
            });
        }
    }
}