using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRTown.Model
{
    public enum OpCodes
    {
        Join = 0,
        Move = 1,
        UpdateMetaData = 2,
        Leave = 3,
    }

    public enum StreamCodes
    {
        Join = 1,
    }
}