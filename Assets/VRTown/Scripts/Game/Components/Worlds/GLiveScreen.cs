using System.Collections;
using System.Collections.Generic;
using agora_gaming_rtc;
using MoonSharp.Interpreter;
using UnityEngine;

namespace VRTown.Game
{
    public class GLiveScreen : MonoBehaviour
    {
        [SerializeField] public string ID;
        [SerializeField] public string Password;
        [SerializeField] public int Width;
        [SerializeField] public int Height;
        [SerializeField] public int Bitrate;
        [SerializeField] public int Orientation;
        [SerializeField] public GameObject Play;

        VideoSurface _surface;
        Material _cacheMaterial = null;
        bool _isInit = false;

        void Start()
        {
            if (!_isInit)
                Setup();
        }

        public void Setup()
        {
            this.gameObject.SetTagRecursively("LiveScreen");
            _surface = this.GetComponent<VideoSurface>();
            if (_surface == null)
                _surface = this.gameObject.AddComponent<VideoSurface>();

            _cacheMaterial = this.GetComponent<MeshRenderer>().material;
            _isInit = true;
        }

        public void SetupScript(Table configData, string id, string password)
        {
            Width = (int)configData.Get("Width").Number;
            Height = (int)configData.Get("Height").Number;
            Bitrate = (int)configData.Get("BitRate").Number;
            Orientation = (int)configData.Get("Orientation").Number;

            ID = id;
            Password = password;
        }

        public void Show()
        {
            Material viewMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            viewMat.color = Color.white;
            this.GetComponent<MeshRenderer>().sharedMaterial = viewMat;
            _surface.SetEnable(true);
            _surface.transform.localScale = new Vector3(1, -1, 1);
            if (Play == null) return;
            Play.gameObject.SetActive(false);
        }

        public void SetupView(uint uid)
        {
            _surface?.SetForUser(uid);
            _surface.SetVideoSurfaceType(AgoraVideoSurfaceType.Renderer);
        }

        public void Hide()
        {
            _surface.SetEnable(false);
            _surface.transform.localScale = new Vector3(1, 1, 1);
            this.GetComponent<MeshRenderer>().material = _cacheMaterial;
            if (Play == null) return;
            Play.gameObject.SetActive(true);
        }
    }
}