using System.Collections;
using System.Collections.Generic;
using Zenject;
using static VRTown.Scene.GSMachine;

namespace VRTown.Game
{
    public partial class GameHome
    {
        public class Installer : Installer<Installer>
        {
            public override void InstallBindings()
            {
                Container.BindInterfacesTo<GameHome>().AsSingle();
            }
        }
    }
}