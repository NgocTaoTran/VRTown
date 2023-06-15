using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTown.Scene;
using VRTown.GameOps;
using VRTown.Model;
using VRTown.Utilities;
using Zenject;

namespace VRTown.Game
{
    public partial class GameFlow : MonoBehaviour
    {
        [Inject] readonly DiContainer Container;

        public static GameFlow Instance { get; private set; }
        private GSMachine _gsMachine = new GSMachine();
        private VRTownApp _stormApp;

        Dictionary<GameState, IBaseState> _gameStates = new Dictionary<GameState, IBaseState>();

        void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            SetupStormApp();

#if !BUILD_DEV || DISABLE_LOGS
            Debug.unityLogger.logEnabled = false;
#endif
            Input.multiTouchEnabled = false;

#if UNITY_STANDALONE && !UNITY_EDITOR
        Screen.SetResolution(720, 1280, false);
#endif

            if (Application.isEditor)
                Application.runInBackground = true;

            Application.targetFrameRate = 60;

#if ENABLE_DEBUG_VIEW
        new GameObject("DebugView", typeof(DebugView));
#endif
        }

        void SetupStormApp()
        {
            _stormApp = new GameObject("StormApp", typeof(VRTownApp)).GetComponent<VRTownApp>();
            Object.DontDestroyOnLoad(_stormApp.gameObject);
        }

        public void ApplyFetchedConfigs()
        {
        }

        public int[] ParseStringToIntArray(string stringToParse, char charSplit)
        {
            int[] array;
            var arrayString = stringToParse.Split(charSplit);
            array = new int[arrayString.Length];
            for (int i = 0; i < arrayString.Length; i++)
                array[i] = int.Parse(arrayString[i]);
            return array;
        }

        IEnumerator Start()
        {
            yield return null;

            // Start game state machine
            _gsMachine.Init(OnStateChanged, GameState.Init);
            while (true)
            {
                _gsMachine.StateUpdate();
                yield return null;
            }
        }

        public void SubscribeAppPause(AppPaused listener)
        {
            _stormApp.SubscribeAppPause(listener);
        }

        public void UnSubscribeAppPause(AppPaused listener)
        {
            _stormApp.UnSubscribeAppPause(listener);
        }

        public T ShowUI<T>(string uiPath, bool overlay = false) where T : UIController
        {
            return UIManager.Instance.ShowUIOnTop<T>(uiPath, overlay);
        }

        public void SceneTransition(System.Action onSceneOutFinished)
        {
            UIManager.Instance.SetUIInteractable(false);
            SceneDirector.Instance.Transition(new TransitionFade()
            {
                duration = 0.667f,
                tweenIn = TweenFunc.TweenType.Sine_EaseInOut,
                tweenOut = TweenFunc.TweenType.Sine_EaseOut,
                onStepOutDidFinish = () =>
                {
                    onSceneOutFinished.Invoke();
                },
                onStepInDidFinish = () =>
                {
                    UIManager.Instance.SetUIInteractable(true);
                }
            });
        }

        #region GSMachine
        GSMachine.UpdateStateDelegate OnStateChanged(System.Enum state)
        {
            return GetState((GameState)state).OnStateEvent;
        }

        IBaseState GetState(GameState state)
        {
            if (!_gameStates.ContainsKey(state))
            {
                _gameStates.Add(state, CreateState(state));
                _gameStates[state].Setup(_gsMachine);
            }
            return _gameStates[state];
        }

        private IBaseState CreateState(GameState state)
        {
            BaseState.Factory baseFactory = Container.ResolveId<BaseState.Factory>(state);
            return baseFactory.Create();
        }
        #endregion
    }
}