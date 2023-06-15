using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRTown.Game
{
    public class EnemyPosition
    {
        public float x;
        public float y;
        public float z;
        public float d;
        public float gx;
        public float gy;
        public long time;
        public EnemyPosition()
        {

        }

        public EnemyPosition(float x, float y, float z, float d, float gx, float gy)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.d = d;
            this.gx = gx;
            this.gy = gy;
        }
    }

    public class EnemyController : MonoBehaviour
    {
    }
}