using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace VRTown.Game
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Input")]
        public PlayerInput Input;

        [Header("Character Input Values")]
        public Vector2 move;
        public Vector2 look;
        public bool jump;
        public bool sprint;
        public bool changeView = false;
        public bool flying = false;

        [Header("Movement Settings")]
        public bool analogMovement;

        [Header("Mouse Cursor Settings")]
        public bool cursorLocked = true;
        public bool cursorInputForLook = true;
        bool _checkLook = false;

        IGameInput _gameInput = null;

        void Start()
        {
            GHelper.GameInput = Input;
        }

        public void RegisterInput(IGameInput input)
        {
            _gameInput = input;
        }

        public void OnMove(InputValue value)
        {
            MoveInput(value.Get<Vector2>());
        }

        public void OnLook(InputValue value)
        {
            if (cursorInputForLook)
            {
                LookInput(value.Get<Vector2>());
            }
        }

        public void OnJump(InputValue value)
        {
            JumpInput(value.isPressed);
        }

        public void OnSprint(InputValue value)
        {
            SprintInput(value.isPressed);
        }

        public void OnChangeView(InputValue value)
        {
            ChangeViewInput(value.isPressed);
        }

        public void OnFly(InputValue value)
        {
            ChangePlayerState(value.isPressed);
        }

        public void OnHome(InputValue value)
        {
            if (value.isPressed)
                _gameInput?.OnHome();
        }

        public void OnEnter(InputValue value)
        {
            _gameInput?.OnEnter(true);
        }

        public void SendMessage(InputValue value)
        {
            _gameInput?.OnSendMessage();
        }

        public void OnMouse(InputValue value)
        {
            _gameInput?.OnMouse();  
        }

        public void MoveInput(Vector2 newMoveDirection)
        {
            move = newMoveDirection * (flying ? 2f : 1f);
        }

        public void LookInput(Vector2 newLookDirection)
        {
            if (Mouse.current.rightButton.isPressed)
            {
                _checkLook = true;
                look = newLookDirection;
            }
        }
        
        public void JumpInput(bool newJumpState)
        {
            jump = newJumpState;
        }

        public void SprintInput(bool newSprintState)
        {
            sprint = newSprintState;
        }

        public void ChangeViewInput(bool isBtnPressed)
        {
            changeView = !changeView;
        }

        public void ChangePlayerState(bool isBtnPressed)
        {
            flying = !flying;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            SetCursorState(cursorLocked);
        }

        private void SetCursorState(bool newState)
        {
            Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
        }

        void Update()
        {
            if (_checkLook && !Mouse.current.rightButton.isPressed)
            {
                look = Vector2.zero;
                _checkLook = false;
            }
        }
    }
}