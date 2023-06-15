using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTown.Game.LuaSystem;
using VRTown.Service;
using Zenject;

namespace VRTown.Game.DemoFeature
{
    public class DemoInstaller : MonoInstaller
    {
        public DemoScene Scene;
        
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<UnityLogger>().AsSingle();
            Container.BindInstance(Scene);
        }
    }
}