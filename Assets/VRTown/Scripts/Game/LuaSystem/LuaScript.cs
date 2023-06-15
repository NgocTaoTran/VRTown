using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using MoonSharp.Interpreter;
using UnityEngine;
using Zenject;
using VRTown.Model;
using System;

namespace VRTown.Game.LuaSystem
{
    public class LuaScript : IPoolable<string, GameObject, IMemoryPool>, System.IDisposable
    {
        #region Zenject_Binding
        [Inject] VRTown.Service.ILogger _logger;
        [Inject] IAgoraController _agoraController;
        [Inject] IModelController _modelController;
        #endregion Zenject_Binding

        GameObject _parent;
        IMemoryPool _pool;
        string _script = "";
        Script _luaScript;
        List<GameObject> _airballs = new List<GameObject>();

        public void LoadScript()
        {
            MoonSharp.Interpreter.UserData.RegisterAssembly();
            Script.DefaultOptions.DebugPrint = s => Debug.LogError(s.ToLower());

            _luaScript = new Script();
            _luaScript.DoString(_script);

            DynValue initFunc = _luaScript.Globals.Get("GetDefines");

            var values = _luaScript.Call(initFunc).ToObject<List<string>>();
            foreach (var value in values)
            {
                switch (value)
                {
                    case "LiveScreen":
                        var tableData = _luaScript.Globals.Get("LiveScreen").Table;
                        var screenId = tableData.Get("GetScreenID").Function.Call().ToObject<string>();
                        var password = tableData.Get("GetPassword").Function.Call().ToObject<string>();

                        var targetGO = tableData.Get("GetTarget").Function.Call().ToObject<string>();
                        var screenTran = _parent.transform.FindInChildren(targetGO).gameObject;

                        if (screenTran != null)
                        {
                            var uiPlayObject = GameObject.Instantiate(Resources.Load<GameObject>("UIPrefabs/Stream/Play"));
                            var uiPlayTransform = uiPlayObject.transform;
                            uiPlayTransform.SetParent(screenTran.transform);
                            uiPlayTransform.localPosition = new Vector3(0, 0, 0.1f);
                            uiPlayTransform.localRotation = Quaternion.identity;
                            uiPlayTransform.localScale = new Vector3(0.3f, 0.3f, 0.3f);

                            var liveScreen = screenTran.AddComponent<GLiveScreen>();
                            liveScreen.GetComponent<GLiveScreen>().Play = uiPlayObject;
                            liveScreen.SetupScript(tableData, screenId, password);
                            _agoraController.RegisterOutput(AgoraService.Streaming, liveScreen);

                            var collider = screenTran.AddComponent<BoxCollider>();
                            collider.center = Vector3.zero;
                            collider.size = new Vector3(14f, 5f, 2.88f);
                        }
                        else
                        {
                            _logger.Error("[LuaScript] LiveScreen Missing target: " + targetGO);
                        }
                        break;
                    case "Water.003":
                        var sphere = _parent.transform.FindInChildren("Water.003").gameObject;
                        sphere.AddComponent<GRotate>();
                        break;
                }
            }
            for (int i = 0; i < 2; i++)
            {
                var root = new GameObject();
                root.gameObject.SetTagRecursively("AirBall");
                root.transform.localPosition = new Vector3(0, 5, 0);
                root.transform.localRotation = Quaternion.identity;
                root.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                _modelController.LoadSingleModel(root, "https://cdn.molly.mana.vn/lands/kkc_billboard.glb.zip/kkc_billboard.glb");
                var coll = root.gameObject.AddComponent<BoxCollider>();
                coll.center = Vector3.zero;
                coll.size = new Vector3(3f, 2f, 2f);
                _airballs.Add(root);
            }
            MoveAround(_airballs[0]);
        }

        public void MoveAround(GameObject gameObject)
        {
            var moveAround = gameObject.transform.gameObject.AddComponent<GMoveAround>();
        }

        public void Dispose()
        {
            _pool.Despawn(this);
        }

        public void OnDespawned()
        {
            _script = null;
            _pool = null;
        }

        public void OnSpawned(string script, GameObject parent, IMemoryPool pool)
        {
            _pool = pool;
            _script = script;
            _parent = parent;
            LoadScript();
        }

        public class Factory : PlaceholderFactory<string, GameObject, LuaScript>
        {
        }
    }
}