using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRTown.Game
{
    public class LocalLand
    {
        public string ID;
        public Vector3 Position;
        public Rect Rect;
        public Transform RootTransform;

        public LocalLand(ModelLocal data)
        {
            ID = data.ID;
            Position = data.Position;
            Rect = data.Rect;
        }
        public static float Overlap(Rect rect1, Rect rect2)
        {
            var dx = Mathf.Min(rect1.xMax, rect2.xMax) - Mathf.Max(rect1.xMin, rect2.xMin);
            var dy = Mathf.Min(rect1.yMax, rect2.yMax) - Mathf.Max(rect1.yMin, rect2.yMin);
            if (dx <= 0 || dy <= 0) return 0f;
            return dx * dy;
        }
    }
}
