using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using VRTown.Model;

namespace VRTown.Game.Component
{
    public class GWorld : MonoBehaviour
    {
        public Vector3Int CenterWorld = new Vector3Int(-1000, -1000);
        Dictionary<string, LandEntity> _currentLands = new Dictionary<string, LandEntity>();
        Dictionary<string, LocalLand> _currentLandsLocal = new Dictionary<string, LocalLand>();
        List<string> _currentLandIDs = new List<string>();

        IModelController _modelController = null;

        public void SetModelController(IModelController controller)
        {
            _modelController = controller;
        }

        public bool ContainLandID(string landId)
        {
            return _currentLandIDs.Contains(landId);
        }

        public void AddLandID(string landId)
        {
            _currentLandIDs.Add(landId);
        }

        public void AddLandEntity(LandEntity entity)
        {
            if (_currentLandIDs.Contains(entity.ID))
            {
                if (!_currentLands.ContainsKey(entity.ID))
                    _currentLands.Add(entity.ID, null);
                _currentLands[entity.ID] = entity;
            }
            else
                ReleaseLand(entity);
        }
        public async void AddLandLocal(ModelLocal model)
        {
            if (!_currentLandsLocal.ContainsKey(model.ID))
                _currentLandsLocal.Add(model.ID, await _modelController.LoadLocalModel(model));
        }

        public void RemoveLands(List<LandEntity> lands)
        {
            foreach (var landEntity in lands)
            {
                _currentLandIDs.Remove(landEntity.ID);
                ReleaseLand(landEntity);
            }
        }

        public void RemoveLocalLands(List<LocalLand> lands)
        {
            foreach (var landEntity in lands)
            {
                ReleaseLocalLand(landEntity);
            }
        }

        async void ReleaseLand(LandEntity landEntity)
        {
            _currentLands.Remove(landEntity.ID);
            await _modelController.StoreLand(landEntity.Entities);
            GameObject.Destroy(landEntity.RootTransform.gameObject);
        }

        void ReleaseLocalLand(LocalLand landEntity)
        {
            landEntity.RootTransform.gameObject.SetActive(false);
        }

        public List<LandEntity> GetOutSideLands(Vector3 offset)
        {
            List<LandEntity> outsideLands = new List<LandEntity>();
            var newBorder = new List<Rect>() { new Rect(offset.x - C.GameConfig.SizeLand, offset.z - C.GameConfig.SizeLand, C.GameConfig.SizeLand * 2, C.GameConfig.SizeLand * 2) };
            foreach (var land in _currentLands)
            {
                if (land.Value == null) continue;
                var _overlap = LandEntity.RectsOverlap(newBorder, land.Value.Rects);
                if (_overlap <= 0.00001f)
                {
                    outsideLands.Add(land.Value);
                }
            }
            return outsideLands;
        }

        public List<LocalLand> GetOutSideLocalLands(Vector3 offset)
        {
            List<LocalLand> outsideLands = new List<LocalLand>();
            var newBorder = new Rect(offset.x - C.GameConfig.SizeLand, offset.z - C.GameConfig.SizeLand, C.GameConfig.SizeLand * 2, C.GameConfig.SizeLand * 2);

            foreach (var land in _currentLandsLocal)
            {
                if (land.Value == null) continue;
                var _overlap = LocalLand.Overlap(newBorder, land.Value.Rect);
                if (_overlap <= 500.0f)
                {
                    outsideLands.Add(land.Value);
                }
            }
            return outsideLands;
        }
        
        public void GetInSideLocalLands(List<LocalLand> outSideLocalLands)
        {
            foreach (var currentLand in _currentLandsLocal.Values)
            {
                if (!outSideLocalLands.Contains(currentLand))
                    currentLand.RootTransform.gameObject.SetActive(true);
            }
        }
    }
}