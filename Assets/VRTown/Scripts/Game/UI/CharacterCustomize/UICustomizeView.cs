using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRTown.Game.UI
{
    public class UIViewData
    {
        public Dictionary<string, object> Parameters;

        public UIViewData()
        {
            Parameters = new Dictionary<string, object>();
        }
    }

    public class UICustomizeView : MonoBehaviour
    {
        public virtual void Setup(UIViewData data)
        {

        }

        public virtual void ReleaseView()
        {

        }
    }
}