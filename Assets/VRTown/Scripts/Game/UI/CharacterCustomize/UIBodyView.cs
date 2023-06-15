using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRTown.Model;

namespace VRTown.Game.UI
{
    public class UIBodyView : UICustomizeView
    {
        [SerializeField] Scrmizu.InfiniteScrollRect _scrollSkins;
        [SerializeField] Toggle _toggleMale;
        [SerializeField] Toggle _togglFamale;
        [SerializeField] Toggle _togglAnime;

        [SerializeField] ToggleGroup _groupColor;

        System.Action<int> _onSkin;
        System.Action<GenderType> _onGender;

        bool isInit = false;

        public override void Setup(UIViewData data)
        {
            _onSkin = (data.Parameters[C.KeyConfigs.KEY_ACTION_COLOR_CHANGED] as System.Action<int>);
            _onGender = (data.Parameters[C.KeyConfigs.KEY_ACTION_GENDER_CHANGED] as System.Action<GenderType>);
            var userData = data.Parameters[C.KeyConfigs.KEY_VALUE_USER_DATA] as UserData;
            _scrollSkins.SetItemData(GetDataColors(userData.skin));

            _toggleMale.SetIsOnWithoutNotify(userData.gender == GenderType.male);
            _togglFamale.SetIsOnWithoutNotify(userData.gender == GenderType.female);
            _togglAnime.SetIsOnWithoutNotify(userData.gender == GenderType.anime);

            isInit = true;
        }

        public List<UIColorData> GetDataColors(int currentValue)
        {
            List<UIColorData> dataColors = new List<UIColorData>();
            for (int i = 0; i < C.ColorConfig.Colors.Length; i++)
                dataColors.Add(new UIColorData(i, i == currentValue, _groupColor, OnSkinChanged));
            return dataColors;
        }

        public void OnSkinChanged(int colorId)
        {
            _onSkin?.Invoke(colorId);
        }

        public void OnGenderChanged(Toggle toggle)
        {
            if (toggle.isOn)
            {
                GenderType type = GenderType.male;
                System.Enum.TryParse<GenderType>(toggle.GetComponent<UIToggleID>().ID, out type);
                _onGender?.Invoke(type);
            }
        }

        public override void ReleaseView()
        {

        }
    }
}