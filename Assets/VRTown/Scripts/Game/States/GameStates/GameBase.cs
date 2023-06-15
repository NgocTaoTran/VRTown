using System.Collections;
using System.Collections.Generic;
using VRTown.Scene;
using UnityEngine;
using Zenject;
using static VRTown.Scene.GSMachine;

namespace VRTown.Game
{
    public interface IBaseState
    {
        void Setup(GSMachine gSMachine);
        void OnStateEvent(StateEvent stateEvent);
    }

    public abstract class BaseState : IBaseState
    {
        public class Factory : PlaceholderFactory<IBaseState>
        { }

        protected GSMachine _gsMachine = null;

        public void Setup(GSMachine gSMachine)
        {
            _gsMachine = gSMachine;
        }

        public abstract void OnStateEvent(StateEvent stateEvent);
    }
}