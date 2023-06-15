using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTown.Model;
using Zenject;

namespace VRTown.Game
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "VRTown/Installers/GameConfig")]
    public class GameConfigInstaller : ScriptableObjectInstaller<GameConfigInstaller>
    {
        public ModelConfig modelConfig;
        public InputConfig inputConfig;

        public override void InstallBindings()
        {
            Container.BindInstance(modelConfig).IfNotBound();
            Container.BindInstance(inputConfig).IfNotBound();
        }
    }
}