
using System.Collections;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ModestTree.Util;
using MoonSharp.Interpreter;
using RenderHeads.Media.AVProVideo;
using UnityEngine;
using UnityEngine.Video;
using VRTown.Game.Utils;

namespace VRTown.Game.LuaSystem
{

    [Preserve]
    public class VectorProxy
    {
        [MoonSharpHidden]
        public Vector3 vec;
        public VectorProxy(Vector3 vec)
        {
            this.vec = vec;
        }

        public static VectorProxy NewVector(float x, float y, float z)
        {
            return new VectorProxy(new Vector3(x, y, z));
        }

        public float x { get { return vec.x; } set { vec.x = value; } }

        public float y { get { return vec.y; } set { vec.y = value; } }

        public float z { get { return vec.z; } set { vec.z = value; } }
    }

    [Preserve]
    public class Entity
    {
        [MoonSharpHidden]
        public GameObject gameObject;
        LuaBehaviour luaBehaviour;
        string basePath;
        IModelController _modelController;

        public override string ToString()
        {
            if (gameObject == null) return base.ToString();
            return $"Entity [{gameObject.name}]";
        }

        public Entity(IModelController modelController)
        {
            _modelController = modelController;
            gameObject = new GameObject();
        }

        public Entity(GameObject gameObject)
        {
            this.gameObject = gameObject;
        }

        public Entity(GameObject gameObject, string basePath)
        {
            this.gameObject = gameObject;
            this.basePath = basePath;
        }

        public Entity clone()
        {
            var clone = GameObject.Instantiate(gameObject);
            return new Entity(clone);
        }

        public VectorProxy position()
        {
            return new VectorProxy(gameObject.transform.position);
        }

        public void position(VectorProxy value)
        {
            gameObject.transform.position = value.vec;
        }

        public void rotation(VectorProxy vec, float angle)
        {
            gameObject.transform.rotation = Quaternion.AngleAxis(angle * 180 / (float)Mathf.PI, vec.vec);
        }

        public DynValue bounds()
        {
            var mesh = gameObject.GetComponent<MeshRenderer>();
            Debug.LogErrorFormat("mesh: {0}", mesh);
            if (mesh == null) return null;
            var bounds = mesh.bounds;
            var ret = DynValue.NewTable(LuaInterface.script);
            ret.Table.Set("min", UserData.Create(new VectorProxy(bounds.min)));
            ret.Table.Set("max", UserData.Create(new VectorProxy(bounds.max)));
            return ret;
        }

        //public VideoPlayerProxy loadVideo(string url)
        //{
        //    VideoPlayer videoPlayer = gameObject.AddComponent<VideoPlayer>();
        //    videoPlayer.url = url;
        //    videoPlayer.Prepare();
        //    return new VideoPlayerProxy(videoPlayer);
        //}
        public AVProVideoPlayerProxy loadVideo(string url)
        {
            MediaPlayer videoPlayer = gameObject.AddComponent<MediaPlayer>();
            ApplyToMaterial apply = gameObject.AddComponent<ApplyToMaterial>();
            var sound3d = gameObject.AddComponent<Sound3D>();

#if !UNITY_EDITOR && UNITY_WEBGL
        // Debug.LogErrorFormat("external library hls");
        // videoPlayer.PlatformOptionsWebGL.externalLibrary = WebGL.ExternalLibrary.HlsJs;
#endif
            videoPlayer.OpenMedia(new MediaPath(url, MediaPathType.AbsolutePathOrURL), autoPlay: true);
            videoPlayer.Loop = true;
            //videoPlayer.url = url;
            //videoPlayer.Prepare();
            sound3d.SetupMedia(videoPlayer);
            return new AVProVideoPlayerProxy(videoPlayer, apply, url);
        }

        async Task loadModelAsync(string path, DynValue callback)
        {
            var loader = new PlotLoader();
            var success = await _modelController.LoadSingleModel(gameObject, path);
            LuaInterface.script.Call(callback.Function, success);
        }

        public void loadModel(string path, DynValue callback)
        {
            //Debug.LogErrorFormat("test1");
            var reg = new Regex("^http(s)?:", RegexOptions.IgnoreCase);
            //Debug.LogErrorFormat("test2 {0}, {1}", reg.Match(path).Success, reg.Match(path));
            if (basePath != null && !reg.Match(path).Success)
            {
                path = basePath + path;
            }
            //Debug.LogErrorFormat("test3 {0}", path);
            _ = loadModelAsync(path, callback);
        }
        public Entity parent()
        {
            var parent = gameObject.transform.parent;
            if (parent == null) return null;
            return new Entity(parent.gameObject);
        }
        public void parent(Entity _parent)
        {
            gameObject.transform.SetParent(_parent.gameObject.transform);
        }

        public string name(string name)
        {
            if (name == null)
            {
                return gameObject.name;
            }
            gameObject.name = name;
            return name;
        }

        public Entity find(string name)
        {
            var child = gameObject.transform.FindInChildren(name);
            if (child == null) return null;
            return new Entity(child.gameObject, basePath);
        }

        public void translate(float x, float y, float z)
        {
            gameObject.transform.Translate(x, y, z);
        }

        public void rotate(float x, float y, float z, float angle)
        {
            gameObject.transform.Rotate(new Vector3(x, y, z), angle);
        }

        public void add(Entity child)
        {
            child.gameObject.transform.SetParent(gameObject.transform);
        }

        static int mainTexPropId = Shader.PropertyToID("_MainTex");
        async Task<Texture> loadTexture(Material material, string path)
        {
            var loader = new PlotLoader();
            var reg = new Regex("^http(s)?:", RegexOptions.IgnoreCase);
            if (basePath != null && !reg.Match(path).Success)
            {
                path = basePath + path;
            }
            Texture texture = await _modelController.LoadSingleTexture(path);
            if (texture != null)
            {
                material.SetTexture(mainTexPropId, texture);
                material.mainTexture = texture;
            }
            return texture;
        }

        IEnumerator playVideo(Material material, string url)
        {
            VideoPlayer videoPlayer = gameObject.AddComponent<VideoPlayer>();
            videoPlayer.url = url;
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.playOnAwake = true;
            videoPlayer.Prepare();
            while (!videoPlayer.isPrepared)
            {
                yield return 0;
            }
            videoPlayer.Play();
            material.mainTexture = videoPlayer.texture;
        }

        IEnumerator playVideoWithPlayer(Material material, VideoPlayerProxy videoPlayer)
        {
            while (!videoPlayer.videoPlayer.isPrepared)
            {
                yield return 0;
            }
            videoPlayer.videoPlayer.Play();
            material.mainTexture = videoPlayer.videoPlayer.texture;
        }

        IEnumerator playVideoWithPlayer(Material material, AVProVideoPlayerProxy videoPlayer)
        {
            videoPlayer.applyToMaterial.Material = material;
            //material.mainTexture = videoPlayer.videoPlayer.texture;
            yield return null;
        }

        public void material(Table mat)
        {
            var material = gameObject.GetComponent<Renderer>().material;
            if (material == null || material.shader.name == "Hidden/InternalErrorShader")
            {
                // material = GLTFast.Materials.ShaderGraphMaterialGenerator.GetDefaultMaterialGenerator().GetDefaultMaterial();
                // gameObject.GetComponent<Renderer>().material = material;
                material = (Material)Resources.Load("MyVideoMappingMaterial");
                gameObject.GetComponent<Renderer>().material = material;
                Debug.LogError($"shader: {material.shader.name}");
            }
            else
            {
                Debug.LogError($"shader: {material.shader.name}");
            }
            foreach (var pair in mat.Pairs)
            {
                switch (pair.Key.String)
                {
                    case "specularIntensity":
                        material.SetFloat("Specular Intensity", (float)pair.Value.Number);
                        break;
                    case "roughness":
                        material.SetFloat("Roughness", (float)pair.Value.Number);
                        break;
                    case "texture":
                        if (pair.Value.String != null)
                        {
                            _ = loadTexture(material, pair.Value.String);
                        }
                        else if (pair.Value.UserData != null && pair.Value.UserData.Object.GetType() == typeof(VideoPlayerProxy))
                        {
                            VideoPlayerProxy proxy = (VideoPlayerProxy)pair.Value.UserData.Object;
                            if (luaBehaviour == null)
                            {
                                luaBehaviour = gameObject.AddComponent<LuaBehaviour>();
                            }

                            luaBehaviour.StartCoroutine(playVideoWithPlayer(material, proxy));
                        }
                        else if (pair.Value.UserData != null && pair.Value.UserData.Object.GetType() == typeof(AVProVideoPlayerProxy))
                        {
                            AVProVideoPlayerProxy proxy = (AVProVideoPlayerProxy)pair.Value.UserData.Object;
                            material = (Material)Resources.Load("MyVideoMappingMaterial");
                            gameObject.GetComponent<Renderer>().material = material;
                            Debug.LogErrorFormat("start assign material {0}", material.name);
                            proxy.applyToMaterial.Material = material;
                            proxy.play();
                            Debug.LogErrorFormat("end assign material {0}", material.name);
                        }
                        break;
                    case "videoTexture":
                        {
                            if (luaBehaviour == null)
                            {
                                luaBehaviour = gameObject.AddComponent<LuaBehaviour>();
                            }
                            luaBehaviour.StartCoroutine(playVideo(material, pair.Value.String));
                            break;
                        }
                }
            }
        }

        public void onUpdate(DynValue func)
        {
            if (luaBehaviour == null)
            {
                luaBehaviour = gameObject.AddComponent<LuaBehaviour>();
            }
            luaBehaviour.func = func;
        }

        public void onClick(DynValue func)
        {
            if (luaBehaviour == null)
            {
                //var collider = gameObject.GetComponentInChildren<Collider>();
                //Debug.LogErrorFormat("Lua file: collider {0}, {1}", collider, gameObject.name);
                //if (collider != null)
                //{
                //    luaBehaviour = collider.gameObject.GetComponent<LuaBehaviour>();
                //    if (luaBehaviour == null)
                //    {
                //        luaBehaviour = collider.gameObject.AddComponent<LuaBehaviour>();
                //        //luaBehaviour = gameObject.AddComponent<LuaBehaviour>();
                //    }
                //}
                luaBehaviour = gameObject.GetComponent<LuaBehaviour>();
                if (luaBehaviour == null)
                {
                    luaBehaviour = gameObject.AddComponent<LuaBehaviour>();
                }
            }
            luaBehaviour.onClick = func;
        }
    }

    [Preserve]
    public class EntityBuilder
    {
        public Entity makeBox(float x, float y, float z)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.localScale = new Vector3(x, y, z);
            return new Entity(cube);
        }

        public Entity makePlane(float width, float height, string axis)
        {
            GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.transform.localScale = new Vector3(width, 1, height);
            switch (axis.ToLower())
            {
                case "x":
                    plane.transform.Rotate(new Vector3(1, 0, 0), 90);
                    plane.transform.Rotate(new Vector3(0, 1, 0), 90);
                    break;
                case "z":
                    plane.transform.Rotate(new Vector3(1, 0, 0), 90);
                    plane.transform.Rotate(new Vector3(0, 1, 0), 180);
                    break;
            }
            return new Entity(plane);
        }
    }
}