using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTown.Model;

namespace VRTown.Game
{
    public class SubLandEntity
    {
        public string Key;
        public Transform Transform;
        public bool Loaded;

        public SubLandEntity(string key)
        {
            Key = key;
            Loaded = false;
        }
    }

    public class LandEntity
    {
        public string ID;
        public List<Rect> Rects;
        public float Left;
        public float Top;
        public List<SubLandEntity> Entities = new List<SubLandEntity>();
        public Transform RootTransform;
        public LandEntity(string id, LandData landData)
        {
            ID = id;
            Rects = new List<Rect>();
            foreach (var bound in landData.metadata.boundaries)
                Rects.Add(new Rect(bound[0], bound[1], bound[2] - bound[0], bound[3] - bound[1]));
        }

        public static float Overlap(Rect rect1, Rect rect2)
        {
            var dx = Mathf.Min(rect1.xMax, rect2.xMax) - Mathf.Max(rect1.xMin, rect2.xMin);
            var dy = Mathf.Min(rect1.yMax, rect2.yMax) - Mathf.Max(rect1.yMin, rect2.yMin);
            if (dx <= 0 || dy <= 0) return 0f;
            return dx * dy;
        }

        public static float RectsOverlap(List<Rect> rects1, List<Rect> rects2)
        {
            float overlap = 0f;
            foreach (var r1 in rects1)
            {
                foreach (var r2 in rects2)
                {
                    overlap += Overlap(r1, r2);
                }
            }

            return overlap;
        }
    }
}