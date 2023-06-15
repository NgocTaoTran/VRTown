using System.Collections.Generic;
using UnityEngine;

namespace VRTown.Model
{
    [System.Serializable]
    public class Land
    {
        public Project project;
        public Scene scene;
    }

    [System.Serializable]
    public class Project
    {
        public string id;
        public string title;
        public string description;
        public Scene Scene;
    }
    [System.Serializable]
    public class Scene
    {
        public string id;
        public Dictionary<string, Entity> entities;
        public Dictionary<string, ComponentInfo> components;
        public Dictionary<string, Asset> assets;
    }


    [System.Serializable]
    public class Entity
    {
        public string id;
        public List<string> components;
        public bool disableGizmos;
        public string name;
    }

    [System.Serializable]
    public class ComponentInfo
    {
        public string id;
        public string type;
        public ComponentData data;
    }

    [System.Serializable]
    public class ComponentData
    {
        public string assetId;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        public Dictionary<string, object> values;
    }

    [System.Serializable]
    public class Asset
    {
        public string id;
        public string legacyId;
        public string assetPackId;
        public string name;
        public string model;
        public string script;
        public string thumbnail;
        public string[] tags;
        public string category;
        public Dictionary<string, string> contents;
        public AssetMetric metrics;
        public object[] parameters;
        public object[] actions;
    }

    [System.Serializable]
    public class AssetMetric
    {
        public int meshes;
        public int bodies;
        public int materials;
        public int textures;
        public int triangles;
        public int entities;
    }

    [System.Serializable]
    public class LandMeta
    {
        public List<float[]> boundaries;
        public float[] offset;
    }

    [System.Serializable]
    public class LandData
    {
        public string id;
        public float left;
        public float top;
        public float right;
        public float bottom;
        public string resource;
        public LandMeta metadata;
    }

    public class LandBorder
    {
        public List<Rect> Borders;

        public LandBorder(List<Rect> rects)
        {
            Borders = rects;
        }

        public bool Overlaps(LandBorder otherBorders)
        {
            foreach (var border in Borders)
            {
                foreach (var otherBorder in otherBorders.Borders)
                {
                    if (otherBorder.Overlaps(border))
                        return true;
                }
            }
            return false;
        }
    }
}