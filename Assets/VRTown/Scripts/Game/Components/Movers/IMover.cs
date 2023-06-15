using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRTown.Game
{
    public interface IMover
    {
        public Vector3 Position { get; }
        void Setup();
        void Teleport(Vector3 pos);
        void Move(float x, float y, float z, float d, float gx = 0, float gy = 0);
        void Teleport(float x, float y, float z, float d, float gx = 0, float gy = 0);
    }
}