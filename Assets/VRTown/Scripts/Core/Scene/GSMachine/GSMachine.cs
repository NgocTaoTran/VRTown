using UnityEngine;
using System.Collections.Generic;

namespace VRTown.Scene
{
    public class GSMachine
    {
        public enum StateEvent
        {
            Enter,
            Execute,
            Exit,
            EnterPop,
        }

        public delegate void UpdateStateDelegate(StateEvent stateEvent);
        public delegate UpdateStateDelegate OnStateChanged(System.Enum state);

        public void Init(OnStateChanged onStateChange, System.Enum initState)
        {
            if (_initialized)
            {
                Debug.LogError("[GSMachine] Init: only called one time!");
                return;
            }

            _onStateChanged = onStateChange;
            _initialized = true;
            _newState = initState;

            ChangeToNewState();
        }

        public void Shutdown()
        {
            _state = null;
            _newState = null;
            _initialized = false;
            _onStateChanged = null;
            _updateStateDelegate = null;
        }

        public void ChangeState(System.Enum state)
        {
            _newState = state;
            StateUpdate();
        }

        public void PushState(System.Enum state)
        {
            _stackStates.Push(_state);
            _newState = state;
            StateUpdate();
        }

        public void PopState()
        {
            if (_state != null)
            {
                _updateStateDelegate(StateEvent.Exit);
            }

            if (_stackStates.Count > 0)
            {
                _state = _newState = _stackStates.Pop();

                var delegate_ = _onStateChanged(_state);
                _updateStateDelegate = delegate_;

                _updateStateDelegate(StateEvent.EnterPop);
            }
        }

        public void StateUpdate()
        {
            if (_initialized)
            {
                if (_state != null && _state.Equals(_newState))
                {
                    _updateStateDelegate(StateEvent.Execute);
                }
                else
                {
                    if (_state != null)
                    {
                        _updateStateDelegate(StateEvent.Exit);
                    }

                    ChangeToNewState();
                }
            }
        }

        public bool IsStateInStack(System.Enum state)
        {
            return _stackStates.Contains(state);
        }

        private void ChangeToNewState()
        {
            _state = _newState;

            var delegate_ = _onStateChanged(_state);
            _updateStateDelegate = delegate_;

            _updateStateDelegate(StateEvent.Enter);
        }

        public System.Enum CurrentState { get { return _state; } }

        private System.Enum _state;
        private System.Enum _newState;
        private UpdateStateDelegate _updateStateDelegate;
        private OnStateChanged _onStateChanged;
        private bool _initialized;
        private Stack<System.Enum> _stackStates = new Stack<System.Enum>();
    }

}
