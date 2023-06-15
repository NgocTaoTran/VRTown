using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRTown.Model
{
    public enum ServerStatus
    {
        Disconnected,
        Connecting,
        Connected,
        JoinMatch,
        Leavematch
    }
}