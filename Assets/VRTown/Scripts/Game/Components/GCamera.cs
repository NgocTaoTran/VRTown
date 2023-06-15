using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

namespace VRTown.Game
{
    public class GCamera : MonoBehaviour
    {
        [SerializeField] CinemachineVirtualCamera _cinemaCamera;

        #region Properties
        public CinemachineVirtualCamera CinemachineCamera { get { return _cinemaCamera; } }
        #endregion Properties

        public void FocusTransform(Transform transform)
        {
            _cinemaCamera.Follow = transform;
        }
    }
}