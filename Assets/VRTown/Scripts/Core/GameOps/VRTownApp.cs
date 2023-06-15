using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRTown.GameOps
{
    public class VRTownApp : MonoBehaviour, IMainAppService
    {
        public event AppPaused OnAppPaused;
        public event System.Action OnAppQuit;
        public event AppPerfChanged OnAppPerfChanged;

        public static VRTownApp Instance
        {
            get;
            private set;
        }

        // FPS
        bool _enableFPSCounter;
        float _FPSUpdateInterval;
        float _FPSAccumulate;
        int _totalFrames;
        float _refreshFPSTimeleft;
        bool _appInLowPerformance;

        public AppConfigs AppConfigs { get; private set; }
        public int AverageFPS { get; private set; }
        public bool IsAppPaused { get; private set; }
        public event System.Action OnDateChanged;

        public void SubscribeAppPause(AppPaused listener)
        {
            OnAppPaused += listener;
        }

        public void UnSubscribeAppPause(AppPaused listener)
        {
            OnAppPaused -= listener;
        }

        public void SubscribeAppQuit(System.Action listener)
        {
            OnAppQuit += listener;
        }

        public void UnSubscribeAppQuit(System.Action listener)
        {
            OnAppQuit -= listener;
        }

        public void SubscribeAppPerfChanged(AppPerfChanged listener)
        {
            OnAppPerfChanged += listener;
        }

        public void UnSubscribeAppPerfChanged(AppPerfChanged listener)
        {
            OnAppPerfChanged -= listener;
        }

        public void SubscribeAppDateChanged(System.Action onDateChanged)
        {
            OnDateChanged += onDateChanged;
        }

        public void UnSubscribeAppDateChanged(System.Action onDateChanged)
        {
            OnDateChanged -= onDateChanged;
        }

        public void SetAppInputActive(bool active)
        {
        }

        void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError("StormStudio instance is already created!");
                return;
            }
            Instance = this;
            Initialize();
        }

        void Update()
        {
            // if (_enableFPSCounter)
            // UpdateFPS();
        }

        void OnDestroy()
        {
            Instance = null;
        }

        void UpdateFPS()
        {
            _refreshFPSTimeleft -= Time.deltaTime;
            _FPSAccumulate += Time.timeScale / Time.deltaTime;
            ++_totalFrames;

            if (_refreshFPSTimeleft <= 0.0f)
            {
                AverageFPS = (int)(_FPSAccumulate / _totalFrames);
                if (AverageFPS < 48 && !_appInLowPerformance)
                {
                    Debug.LogWarning("App is running in low performance!");

                    _appInLowPerformance = true;
                    ProcessAppPerfChanged(true);
                }
                else if (AverageFPS >= 48 && _appInLowPerformance)
                {
                    Debug.LogWarning("App back to normal performance!");

                    _appInLowPerformance = false;
                    ProcessAppPerfChanged(false);
                }

                _refreshFPSTimeleft = _FPSUpdateInterval;
                _FPSAccumulate = 0.0f;
                _totalFrames = 0;
            }
        }

        void ProcessAppPerfChanged(bool lowPerf)
        {
            OnAppPerfChanged?.Invoke(lowPerf);
        }

        public void RunOnMainThread(System.Action action)
        {
#if UNITY_ANDROID
            System.Threading.Tasks.Task.Run(() => { }).ContinueWithOnMainThread(task =>
            {
                action();
            });
#else
            action();
#endif
        }

        void Initialize()
        {
            // Load app configs
            var textAssets = Resources.Load<TextAsset>("app_configs");
            AppConfigs = new AppConfigs();
            AppConfigs.Load(textAssets.bytes);

            // FPS
            _enableFPSCounter = AppConfigs.GetValue("enable_fps_counter", "General").BooleanValue;
            if (_enableFPSCounter)
            {
                _FPSUpdateInterval = (float)AppConfigs.GetValue("fps_update_interval", "General").DoubleValue;
                _refreshFPSTimeleft = _FPSUpdateInterval;
            }

#if UNITY_IOS
            StartCoroutine(EnablePerformanceMonitor());
#endif
        }

        #region Performance Monitor
#if UNITY_IOS
        IEnumerator EnablePerformanceMonitor()
        {
            while (!IsFirebaseReady)
                yield return null;

            _monitorInterstitialAdLoadEnabled = AppConfigs.GetValue("monitor_interstitial_ad_load", "PerformanceMonitor").BooleanValue;
            _monitorRewardedAdLoadEnabled = AppConfigs.GetValue("monitor_rewarded_ad_load", "PerformanceMonitor").BooleanValue;
        }
#endif
        #endregion

        #region Interrupt
        void ProcessAppPaused(bool pauseStatus)
        {
            OnAppPaused?.Invoke(pauseStatus);
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus != IsAppPaused)
                ProcessAppPaused(pauseStatus);

            IsAppPaused = pauseStatus;
        }

        private void OnApplicationFocus(bool focusStatus)
        {
            if (focusStatus == IsAppPaused)
                ProcessAppPaused(!focusStatus);

            IsAppPaused = !focusStatus;
        }

        private void OnApplicationQuit()
        {
            OnAppQuit?.Invoke();
        }
        #endregion
    }
}
