using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using VRTown.Game.UI;
using VRTown.Model;
using VRTown.Network;
using VRTown.Scene;
using VRTown.Service;
using Zenject;
using static VRTown.Scene.GSMachine;

namespace VRTown.Game
{
    public partial class GameInit : BaseState, IGameInit
    {
        #region Zenject_Binding
        [Inject] VRTown.Service.ILogger _logger;
        [Inject] IUserManager _userManager;
        [Inject] IGameController _gameController;
        [Inject] ConfigController _configController;
        [Inject] LocalizationManager _localizationManager;
        #endregion Zenject_Binding

        #region Properties
        #endregion Properties

        #region Local
        SplashScreenUI _splashScreenUI;
        #endregion Local

        public override async void OnStateEvent(StateEvent stateEvent)
        {
            if (stateEvent == StateEvent.Enter)
            {
                await _localizationManager.Initialize();
                GHelper.Localization = _localizationManager;

                _splashScreenUI = UIManager.Instance.ShowUIOnTop<SplashScreenUI>("SplashScreenUI");
                _splashScreenUI.Setup();
                _splashScreenUI.SetProgress(0.1f);

                await LoadConfig();
                await InitGame();
            }
            else if (stateEvent == StateEvent.Exit)
            {
                UIManager.Instance.ReleaseUI(_splashScreenUI, true);
                _logger.Log("[GAME] GameInit: State Exit");
            }
        }

        public async UniTask LoadConfig()
        {
            await _configController.Initialize();
            _splashScreenUI.SetProgress(0.6f);
        }

        public async UniTask InitGame()
        {
            await _userManager.Initialize();
            _splashScreenUI.SetProgress(0.75f);
            await _gameController.Initialize();
            _splashScreenUI.SetProgress(1f);
            OnInitFinished();
        }

        public void OnInitFinished()
        {
            GameFlow.Instance.SceneTransition(() =>
            {
                _gsMachine.ChangeState(GameState.Login);
            });
        }
    }
}