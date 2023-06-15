using System.Collections;
using System.Collections.Generic;
using ModestTree.Util;
using MoonSharp.Interpreter;
using RenderHeads.Media.AVProVideo;
using UnityEngine;
using UnityEngine.Video;
using Zenject;

namespace VRTown.Game.LuaSystem
{
    [Preserve]
    public class PlayerProxy
    {
        GameObject player;
        private GameObject getPlayer()
        {
            if (player == null)
            {
                player = GameObject.Find("PlayerArmature");
            }
            return player;
        }

        public DynValue position()
        {
            var position = DynValue.NewTable(LuaInterface.script);
            var pos = getPlayer().transform.position;
            position.Table.Set("x", DynValue.NewNumber(pos.x));
            position.Table.Set("y", DynValue.NewNumber(pos.y));
            position.Table.Set("z", DynValue.NewNumber(pos.z));
            return position;
        }

        public void teleport(float x, float y, float z)
        {
            // var player = getPlayer();
            // var playerController = player.GetComponent<ThirdPersonController>();
            // playerController.SetPosition(new Vector3(x, playerController.position().y, z), 0);
        }

        public DynValue account()
        {
            DynValue tableAccount = DynValue.NewTable(LuaInterface.script);
            // var account = GameClient.account;
            // tableAccount.Table.Set("user_id", DynValue.NewNumber(account.CustomId.GetHashCode()));
            // tableAccount.Table.Set("username", DynValue.NewString(account.User.Username));
            return tableAccount;
        }
    }

    public class LuaInterface
    {
        #region Zenject_Binding
        readonly VRTown.Service.ILogger _logger;
        readonly IModelController _modelController;
        #endregion Zenject_Binding

        public static Script script;

        [Inject]
        public LuaInterface(VRTown.Service.ILogger logger, IModelController controller)
        {
            _logger = logger;
            _modelController = controller;

            _logger.Log("[LUA] Init Lua Interface!");
            script = new Script();
        }

        public static void LogFormat(string text, params object[] objs)
        {
            Debug.LogFormat(text, objs);
        }

        public static void LogErrorFormat(string text, params object[] objs)
        {
            Debug.LogErrorFormat(text, objs);
        }

        public void RegisterSimpleAction<T1, T2>()
        {
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(System.Action<T1, T2>),
                v =>
                {
                    var function = v.Function;
                    return (System.Action<T1, T2>)((T1 p1, T2 p2) => function.Call(p1, p2));
                }
            );
        }

        public void RegisterSimpleFunc<T>()
        {
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(System.Func<T>),
                v =>
                {
                    var function = v.Function;
                    return (System.Func<T>)(() => function.Call().ToObject<T>());
                }
            );
        }

        bool registered = false;
        public void RunLuaScript(GameObject root, string scriptText, Dictionary<string, object> values = null)
        {
            var wrapScript = string.Format(@"
                return function(root, param_values)
                    local {0}
                    init(root, param_values)
                end", scriptText);

            Debug.LogErrorFormat("script.lua {0}", wrapScript);
            if (registered == false)
            {
                registered = true;
                UserData.RegisterAssembly();
                //Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.UserData, typeof(EntityBuilder));
                //Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.UserData, typeof(PlayerProxy));
                //Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.UserData, typeof(Entity));
                RegisterSimpleAction<string, object[]>();
                RegisterSimpleFunc<Entity>();
                UserData.RegisterType<EntityBuilder>();
                UserData.RegisterType<PlayerProxy>();
                UserData.RegisterType<VideoPlayerProxy>();
                UserData.RegisterType<AVProVideoPlayerProxy>();
                UserData.RegisterType<Entity>();
                UserData.RegisterType<VectorProxy>();
                UserData.RegisterType<LuaUI>();
                UserData.RegisterType<LuaUIContainer>();
                UserData.RegisterType<LuaUIView>();
                UserData.RegisterType<LuaUIButton>();
                script.Globals["log"] = (System.Action<string, object[]>)LogFormat;
                script.Globals["logError"] = (System.Action<string, object[]>)LogErrorFormat;
                script.Globals["Builder"] = new EntityBuilder();
                script.Globals["Player"] = new PlayerProxy();
                script.Globals["UI"] = new LuaUI();
                script.Globals["vector"] = (System.Func<float, float, float, VectorProxy>)VectorProxy.NewVector;
                script.Globals["timeSeconds"] = (System.Func<float>)(() => (float)((System.DateTimeOffset.Now.ToUnixTimeMilliseconds() % 864000000L) / 1000.0));
                script.Globals["Entity"] = (System.Func<Entity>)(() => new Entity(_modelController));
                script.Globals["screenWidth"] = (System.Func<float>)(() => Screen.width);
                script.Globals["screenHeight"] = (System.Func<float>)(() => Screen.height);
            }
            DynValue res = script.DoString(wrapScript);
            if (values == null)
            {
                script.Call(res, new Entity(root, ""));
            }
            else
            {
                Debug.LogErrorFormat("values is not null {0} {1}", values, values["x"]);
                script.Call(res, new Entity(root, ""), values);
            }
        }
    }
}