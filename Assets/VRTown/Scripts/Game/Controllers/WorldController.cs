using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using VRTown.Game.Component;
using VRTown.Game.UI;
using VRTown.Model;
using VRTown.Network;
using Zenject;

namespace VRTown.Game
{

    public interface IWorldTransofrmListener
    {

    }

    public interface IWorldController
    {
        UniTask Initialize();
        void LoadWorld(Vector3 offset);
    }

    public class WorldController : IWorldController, IWorldTransofrmListener
    {
        #region Zenject_Binding
        [Inject] VRTown.Service.ILogger _logger;
        [Inject] readonly IModelController _modelController;
        [Inject] readonly Models _modelData;
        [Inject] IGameServer _gameServer;
        #endregion Zenject_Binding

        #region Locals
        GWorld _gWorld = null;
        #endregion Locals

        public UniTask Initialize()
        {
            var objWorld = new GameObject();
            _gWorld = objWorld.AddComponent<GWorld>();
            _gWorld.SetModelController(_modelController);
            _gWorld.transform.position = Vector3.zero;
            return default;
        }

        public async void LoadWorld(Vector3 worldPos)
        {
            var mapPos = C.MapConfig.ConvertMapPosition(worldPos);
            if (_gWorld.CenterWorld != mapPos)
            {
                try
                {
                    _gWorld.CenterWorld = mapPos;
                    var lands = await GetLands(worldPos);

                    List<LandData> newLands = new List<LandData>();
                    foreach (var land in lands)
                    {
                        if (_gWorld.ContainLandID(land.id)) continue;
                        newLands.Add(land);
                    }
                    var outsideLands = _gWorld.GetOutSideLands(worldPos);
                    _gWorld.RemoveLands(outsideLands);
                    // var outsideLocalLands = _gWorld.GetOutSideLocalLands(worldPos);
                    // _gWorld.RemoveLocalLands(outsideLocalLands);

                    foreach (var land in newLands)
                    {
                        _gWorld.AddLandID(land.id);
                        _gWorld.AddLandEntity(await _modelController.CreateLand(land));
                    }

                    // foreach (var localLand in _modelData.ListModel)
                    // {
                    //     _gWorld.AddLandLocal(localLand);
                    //     _gWorld.GetInSideLocalLands(outsideLocalLands);
                    // }
                }
                catch (Exception e)
                {
                    Debug.LogError("exception " + e.Message);
                }
            }
        }
        async UniTask<List<LandData>> GetLands(Vector3 worldPos)
        {
            var rect = new Rect(worldPos.x - C.GameConfig.SizeLand, worldPos.z - C.GameConfig.SizeLand, C.GameConfig.SizeLand * 2, C.GameConfig.SizeLand * 2);
            var payload = new Dictionary<string, float> {
                { "left", rect.xMin },
                { "top", rect.yMin },
                { "right", rect.xMax },
                { "bottom", rect.yMax },
            };

            var result = await _gameServer.RequestRPC(C.ServerConfig.NAME_RPC_LOAD_LAND, JsonConvert.SerializeObject(payload));
            var lands = JsonConvert.DeserializeObject<List<LandData>>(result.Payload);
            return lands;
        }
    }
}