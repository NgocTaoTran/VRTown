using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRTown.Model
{
    public class BuilderNode
    {
        public Transform transform;
        public Dictionary<string, object> values;
    }

    public class LoadModelTask
    {
        public List<BuilderNode> objectParent;
        public List<string> fullPaths;
        public int loadId;
    }

    public enum AssetType
    {
        Unknow = 0,
        Texture = 1,
        Model = 2,
        Audio = 3,
        Script = 4
    }

    public class RequestRemoteAsset
    {
        public string URL;
        public AssetType Type;
        public Dictionary<string, object> Metadata;

        public RequestRemoteAsset(string url, Dictionary<string, object> metadata = null)
        {
            URL = url;
            Type = ParseType(System.IO.Path.GetExtension(url));
            Metadata = metadata;
        }

        AssetType ParseType(string extension)
        {
            switch (extension)
            {
                case "glb":
                case "gltf":
                    return AssetType.Model;

                case "png":
                case "jpg":
                    return AssetType.Texture;

                case "mp3":
                case "wav":
                    return AssetType.Audio;

                default:
                    return AssetType.Unknow;
            }
        }
    }
}