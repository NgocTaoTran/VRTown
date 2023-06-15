using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRTown.Network;
using VRTown.Scene;

namespace VRTown.Game.UI
{
    public class SplashScreenUI : UIController
    {
        [SerializeField] Image _fillProgress;
        [SerializeField] TMP_Text _textVersion;

        public void Setup()
        {
            var environment = EnvironmentManager.GetEnvironment();
            _textVersion.text = $"Version: ({EnvironmentManager.Environment}) " + Application.version;
        }

        public void SetProgress(float percent)
        {
            _fillProgress.fillAmount = percent;
        }
    }
}