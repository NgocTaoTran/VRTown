using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRTown.Game
{
    public class GUserMover : MonoBehaviour, IMover
    {
        ThirdPersonController Controller
        {
            get
            {
                if (_thirdPersonController == null)
                    _thirdPersonController = GetComponent<ThirdPersonController>();
                return _thirdPersonController;
            }
        }

        ThirdPersonController _thirdPersonController = null;

        public Vector3 Position
        {
            get
            {
                return Controller.position();
            }
        }

        public void Setup()
        {

        }

        public void Move(float x, float y, float z, float d, float gx = 0, float gy = 0)
        {

        }

        public void Teleport(float x, float y, float z, float d, float gx = 0, float gy = 0)
        {
            Controller.SetPosition(new Vector3(x, z, y), d);
        }

        public void Teleport(Vector3 pos)
        {
            Controller.SetPosition(pos, 0f);
        }

        public void Jump(int id, bool value)
        {
            throw new System.NotImplementedException();
        }
    }
}