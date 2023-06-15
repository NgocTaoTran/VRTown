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
    public partial class GameLogin : BaseState, IGameLogin
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
        LoginUI _loginUI;

        #endregion Local

        public override void OnStateEvent(StateEvent stateEvent)
        {
            if (stateEvent == StateEvent.Enter)
            {
                _loginUI = UIManager.Instance.ShowUIOnTop<LoginUI>("LoginUI");
                _loginUI.Setup(onLoginWallet, onLoginEmail);
                _agoraController.Initialize();
            }
            else if (stateEvent == StateEvent.Exit)
            {
                _logger.Log("[GAME] GameLogin: State Exit");
                UIManager.Instance.ReleaseUI(_loginUI, true);
            }
        }

        public async void onLoginWallet()
        {
            await _walletController.ConnectWallet();

            string address = _walletController.GetWalletAddress() ?? "0x8325BA35280E128C01F475493ecd28A1999b0874";

            await _gameServer.Authenticate(_gameController, address);

            if (_gameServer.IsNewProfile)
            {
                _userManager.CreateRandom(_gameServer.UserNID);
                var changeNameUI = UIManager.Instance.ShowUIOnTop<ChangeNameUI>("ChangeNameUI");
                changeNameUI.Setup(_userManager.UserID, onChangeName);
            }
            else
            {
                _userManager.LoadProfile(_gameServer.Account);
                await _gameServer.ConnectSocket(onConnected);
            }
            GHelper.UserNID = _gameServer.UserNID;
        }

        public async void onLoginEmail(string userId)
        {
            await _gameServer.Authenticate(_gameController, userId);
            if (_gameServer.IsNewProfile)
            {
                _userManager.CreateRandom(_gameServer.UserNID);
                var changeNameUI = UIManager.Instance.ShowUIOnTop<ChangeNameUI>("ChangeNameUI");
                changeNameUI.Setup(_userManager.UserName, onChangeName);
            }
            else
            {
                _userManager.LoadProfile(_gameServer.Account);
                await _gameServer.ConnectSocket(onConnected);
            }
            GHelper.UserNID = _gameServer.UserNID;
        }

        public async void onChangeName(string newName)
        {
            // Sau khi nhap Name
            _userManager.SetDisplayName(newName); // => Set Name moi vao profile
            // await _gameServer.OnSendMatchStateAsync(_gameServer.MatchID, (long)OpCodes.UpdateProfile, JsonConvert.SerializeObject(_userManager.Profile));
            // await _gameServer.UpdateAccount(_gameServer.Account.User.Metadata); // => Save lai profile len Nakama Console, ham nay chi truyen Name nen gio doi, no se truyen cai UserData len
            await _gameServer.ConnectSocket(onConnected);
            // bay gio di test coi data no day len
        }

        void onConnected()
        {
            GameFlow.Instance.SceneTransition(() =>
            {
                _gsMachine.ChangeState(GameState.GameHome);
            });
        }
    }
}