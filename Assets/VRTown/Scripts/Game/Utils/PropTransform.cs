using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRTown.Game
{
    public class PropTransform : MonoBehaviour
    {
        public string ParentName;
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;

        public void SetupTransform(Transform transform)
        {
            Position = transform.localPosition;
            Rotation = transform.localRotation;
            Scale = transform.localScale;
        }
    }
}