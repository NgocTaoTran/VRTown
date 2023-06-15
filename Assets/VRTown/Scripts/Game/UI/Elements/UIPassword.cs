using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace VRTown.Game.UI
{
    public class UIPassword : MonoBehaviour
    {
        [Header("Main View")]
        [SerializeField] GameObject _btnLeave;

        [Header("Password View")]
        [SerializeField] TMP_Text _textPassword;
        [SerializeField] List<Button> _btnNumbers;

        string _password;
        string _userPassword;
        opencloseDoor _opencloseDoor = null;
        System.Action<opencloseDoor> _onOpenDoor;


        public void Setup(opencloseDoor opencloseDoor ,System.Action<opencloseDoor> onOpenDoor)
        {
            _password = "2222";
            _btnLeave.SetActive(true);
            _opencloseDoor = opencloseDoor;
            _onOpenDoor = onOpenDoor;
        }

        public void Show()
        {
            this.GetComponent<RectTransform>().DOAnchorPosX(-50, 1f);
            _textPassword.text = "";
            _userPassword = "";

        }

        public void Hide()
        {
            _password = "";
            _textPassword.text = "";
            _userPassword = "";
            this.GetComponent<RectTransform>().DOAnchorPosX(350f, 1f);
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
            Hide();
        }

        public void TouchedOK()
        {
            if (_userPassword == _password)
            {
                Hide();
                _onOpenDoor?.Invoke(_opencloseDoor);
            }
            else
            {
                ShowPasswordError();
            }
        }
    }
}