using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using VRTown.Model;

namespace VRTown.Game.UI
{
    public class UIStreamView : MonoBehaviour
    {
        [Header("Main View")]
        [SerializeField] TMP_Text _titleScreen;
        [SerializeField] GameObject _btnLeave;
        [SerializeField] GameObject _btnClose;

        [Header("Password View")]
        [SerializeField] RectTransform _tfPassword;
        [SerializeField] TMP_Text _textPassword;
        [SerializeField] List<Button> _btnNumbers;

        string _password;
        string _userPassword;
        public AgoraService Type { get { return AgoraService.Streaming; } }
        System.Action<string> _onHost;
        System.Action<string> _onAudience;
        System.Action<AgoraService> _onLeave;
        System.Action<bool> _onEnableListAudience;


        public void Setup(string name, string password, System.Action<string> onHost, System.Action<string> onAudience, System.Action<AgoraService> onLeave, System.Action<bool> onEnableListAudience)
        {
            _password = password;
            _onHost = onHost;
            _onAudience = onAudience;
            _onLeave = onLeave;
            _onEnableListAudience = onEnableListAudience;
            _btnLeave.SetActive(false);
            _titleScreen.text = name;
        }

        public void Show()
        {
            this.GetComponent<RectTransform>().DOAnchorPosX(-50, 1f);
        }

        public void Hide()
        {
            _password = "";
            this.GetComponent<RectTransform>().DOAnchorPosX(350f, 1f);
        }

        public void EnableLeave(bool enabled)
        {
            _btnLeave.SetActive(enabled);
        }

        public void TouchedHost()
        {
            ShowPassword();
        }

        public void TouchedAudience()
        {
            _btnLeave.SetActive(true);
            _onAudience?.Invoke(_titleScreen.text);
             _onEnableListAudience?.Invoke(true);
        }

        public void TouchedLeave()
        {
            _btnLeave.SetActive(false);
            _btnClose.SetActive(true);
            _onLeave?.Invoke(Type);
            _onEnableListAudience?.Invoke(false);
        }

        public void ShowPassword()
        {
            _userPassword = "";
            _textPassword.text = _userPassword;

            foreach (var btn in _btnNumbers)
                btn.interactable = false;

            Sequence seq = DOTween.Sequence();
            seq.Append(_tfPassword.DOAnchorPosY(0, 0.3f));
            seq.Join(_tfPassword.GetComponent<CanvasGroup>().DOFade(1f, 0.3f));
            seq.OnComplete(() =>
            {
                foreach (var btn in _btnNumbers)
                    btn.interactable = true;
            });
        }

        public void HidePassword()
        {
            Sequence seq = DOTween.Sequence();
            seq.Append(_tfPassword.DOAnchorPosY(-215, 0.2f));
            seq.Join(_tfPassword.GetComponent<CanvasGroup>().DOFade(0f, 0.2f));
            seq.OnComplete(() =>
            {
                foreach (var btn in _btnNumbers)
                    btn.interactable = false;
            });
        }

        public void ShowPasswordError()
        {
            _userPassword = "";
            foreach (var btn in _btnNumbers)
                btn.interactable = false;
            _textPassword.text = "WRONG!";
            _textPassword.characterSpacing = 0;
            _textPassword.color = Color.red;
            _textPassword.transform.localScale = Vector3.zero;
            _textPassword.transform.DOScale(Vector3.one, 0.3f).OnComplete(() =>
            {
                _textPassword.text = "";
                _textPassword.characterSpacing = 10;
                _textPassword.transform.localScale = Vector3.one;
                _textPassword.color = Color.black;
                foreach (var btn in _btnNumbers)
                    btn.interactable = true;
            });
        }

        public void TouchedNumber(string number)
        {
            if (_userPassword.Length > 5) return;
            _userPassword += number;
            _textPassword.text = _userPassword;
        }

        public void TouchedRemove()
        {
            if (_userPassword.Length > 0)
                _userPassword = _userPassword.Substring(0, _userPassword.Length - 1);
            _textPassword.text = _userPassword;
        }

        public void TouchedClosePassword()
        {
            HidePassword();
        }

        public void TouchedOK()
        {
            if (_userPassword == _password)
            {
                HidePassword();
                _btnLeave.SetActive(true);
                _btnClose.SetActive(false);
                _onHost?.Invoke(_titleScreen.text);
                _onEnableListAudience?.Invoke(true);
            }
            else
            {
                ShowPasswordError();
            }
        }
    }
}