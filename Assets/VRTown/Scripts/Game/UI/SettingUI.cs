using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRTown.Scene;

namespace VRTown.Game.UI
{
    public class SettingUI : UIController
    {
        [System.Serializable]
        public class LocalizeToggle
        {
            public SystemLanguage ID;
            public Toggle Toggle;
        }

        [SerializeField] List<LocalizeToggle> _languages;

        System.Action<SystemLanguage> _onLanguage;
        System.Action _onClose = null;
        
        bool _isInit = false;

        public void Setup(SystemLanguage curLanguage, System.Action<SystemLanguage> onLanguage, System.Action onClose)
        {
            _onLanguage = onLanguage;
            _onClose = onClose;

            foreach (var language in _languages)
                language.Toggle.SetIsOnWithoutNotify(language.ID == curLanguage);
            _isInit = true;
        }

        public void TouchedLanguage(Toggle toggle)
        {
            if (!_isInit) return;
            if (toggle.isOn)
            {
                var language = _languages.Find(val => val.Toggle == toggle).ID;
                _onLanguage?.Invoke(language);
            }
        }

        public void TouchedClose()
        {
            _onClose?.Invoke();
            UIManager.Instance.ReleaseUI(this, true);
        }
    }
}