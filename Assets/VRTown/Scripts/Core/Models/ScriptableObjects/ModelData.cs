using System.Collections.Generic;
using UnityEngine;
using VRTown.Model;
using Zenject;

namespace VRTown.Game
{

    [CreateAssetMenu(fileName = "ModelData", menuName = "VRTown/Installers/ModelData", order = 1)]
    public class ModelData : ScriptableObjectInstaller<ModelData>
    {
        public Models models;
        public override void InstallBindings()
        {
            Container.BindInstance(models).IfNotBound();
        }
    }

    [System.Serializable]
    public class ModelLocal
    {
        public string ID;
        public Vector3 Position;
        public Rect Rect;

        public ModelLocal(string id, Vector3 pos)
        {
            ID = id;
            Position = pos;
        }
    }

    [System.Serializable]
    public class Models
    {
        public List<ModelLocal> ListModel = new List<ModelLocal>();
    }
    
}