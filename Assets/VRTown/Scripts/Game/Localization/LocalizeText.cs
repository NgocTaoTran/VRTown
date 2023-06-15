using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace VRTown.Game
{
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizeText : MonoBehaviour
    {
        [SerializeField] public string KeyLocalization;

        protected TMP_Text Text
        {
            get
            {
                if (_tmpText == null)
                    _tmpText = GetComponent<TMP_Text>();
                return _tmpText;
            }
        }

        TMP_Text _tmpText = null;

        void OnEnable()
        {
            GHelper.Localization.LocalizationChanged += onTextChanged;
            onTextChanged();
        }

        void OnDisable()
        {
            GHelper.Localization.LocalizationChanged -= onTextChanged;
        }

        void onTextChanged()
        {
            Text.text = GHelper.Localization.Localize<string>(KeyLocalization);
        }
    }
}