using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IState
{
    void Setup(StateData stateData = null);
    void AssignAnimationIDs();
}
