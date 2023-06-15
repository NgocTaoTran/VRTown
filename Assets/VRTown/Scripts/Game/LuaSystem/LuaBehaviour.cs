using MoonSharp.Interpreter;
using UnityEngine;
using VRTown.Game.Utils;

namespace VRTown.Game.LuaSystem
{
    class LuaBehaviour : MonoBehaviour
    {
        public DynValue func;
        public DynValue onClick;

        public LuaBehaviour()
        {
        }

        //private void OnMouseDown()
        //{
        //    Debug.LogError("Lua file: on click run");
        //    if (onClick != null && onClick.Function != null)
        //    {
        //        LuaInterface.script.Call(onClick.Function);
        //    }
        //}

        bool IsChild(Transform child, Transform parent)
        {
            while (child != null)
            {
                Debug.LogFormat("child: {0}, parent {1}", child, parent);
                if (child == parent) return true;
                child = child.parent;
            }
            return false;
        }

        void Update()
        {
            if (func != null && func.Function != null)
            {
                LuaInterface.script.Call(func.Function, Time.deltaTime);
            }
            if (onClick != null && onClick.Function != null)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, 30))
                    {
                        if (IsChild(hit.collider.gameObject.transform, gameObject.transform))
                        {
                            LuaInterface.script.Call(onClick.Function);
                        }
                    }
                }
            }
        }
    }
}