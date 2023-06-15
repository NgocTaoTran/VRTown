using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRTown.Scene;
using Nakama;
using System;

namespace VRTown.Game.UI
{
    public class LoginUI : UIController
    {
        public enum StateView
        {
            Main,
            Metamask,
            Email,
            ViewMatch
        }
        [SerializeField] GameObject _goMain;
        [SerializeField] GameObject _goMetamask;
        [SerializeField] GameObject _goEmail;
        [SerializeField] GameObject _goViewMath;

        [SerializeField] TextMeshProUGUI _promptTitle;
        [SerializeField] TextMeshProUGUI _promptInputTitle;
        [SerializeField] TMPro.TMP_InputField _promptInput;
        [SerializeField] Button _btnLogin;


        System.Action _onTouchWallet;
        System.Action<string> _onTouchPlay;

        StateView _currentView = StateView.Email;

        public void Setup(System.Action onWallet, System.Action<string> onTouchPlay)
        {
            _onTouchPlay = onTouchPlay;
            _onTouchWallet = onWallet;
            EnableLoginView(StateView.Main);
        }

        public void TouchedMetaMask()
        {
            EnableLoginView(StateView.Metamask);
            _onTouchWallet?.Invoke();
        }

        public void TouchedEmail()
        {
            EnableLoginView(StateView.Email);
            _promptInput.ActivateInputField();
        }

        protected override void OnBack()
        {
            EnableLoginView(StateView.Main);
        }

        public void EnableLoginView(StateView stateView)
        {
            if (_currentView == stateView) return;
            _currentView = stateView;
            _goMain.SetActive(_currentView == StateView.Main);
            _goMetamask.SetActive(_currentView == StateView.Metamask);
            _goEmail.SetActive(_currentView == StateView.Email);
        }

        public void TouchedPlay()
        {
            _btnLogin.interactable = false;
            _onTouchPlay?.Invoke(_promptInput.text);
        }
    }
}