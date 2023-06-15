using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRTown.Model
{
    public enum BundleLoaderName
    {
        Resource = 0,
        Addressable = 1,
        Zip
    }

    public enum LoadState
    {
        NotLoad,
        Loading,
        Loaded
    }
}