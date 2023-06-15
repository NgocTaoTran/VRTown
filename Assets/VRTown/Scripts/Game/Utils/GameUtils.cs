using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace VRTown.Game
{
    public static class GameUtils
    {
        public async static void Delay(float delay, System.Action cb = null)
        {
            await UniTask.Delay(delay);
            cb?.Invoke();
        }

        public static Vector3 GetPositonByArray(double[] position)
        {
            if (position == null)
                return Vector3.zero;
            return new Vector3((float)position[0], (float)position[1], (float)position[2]);
        }

        public static double[] ConvertVector3ToArray(Vector3 pos)
        {
            var outPut = new double[3];
            outPut[0] = pos.x;
            outPut[1] = pos.y;
            outPut[2] = pos.z;
            return outPut;
        }
    }
}