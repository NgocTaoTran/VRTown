using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VRTown.Utilities;

namespace VRTown.Scene
{
    public class TransitionCrossFade : SceneTransitionConcurrent
    {
        public float duration;
        public TweenFunc.TweenType tweenIn;

        private ActionEaseInterval _easeFadeIn;

        private GameObject _objEffectBG;
        private Material _materialBG;

        private GameObject _objEffectFG;
        private Material _materialFG;
        private RenderTexture _renderTexture;
        private bool _startRecording;
        private List<Camera> _recordCameras = new List<Camera>();

        public override void Enter()
        {
            base.Enter();

            _easeFadeIn = new ActionEaseInterval()
            {
                duration = this.duration * 0.5f,
                tweenType = tweenIn,
                easingParam = null
            };

            // create overlay objects
            _objEffectBG = new GameObject("TransitionCrossFadeBG");
            _objEffectBG.layer = SceneDirector.OverlayLayer;
            _objEffectBG.transform.parent = SceneDirector.Instance.transform;
            var meshFilter = _objEffectBG.AddComponent<MeshFilter>();
            meshFilter.mesh = SceneDirector.Instance.CreateQuadMesh();

            _objEffectFG = new GameObject("TransitionCrossFadeFG");
            _objEffectFG.layer = SceneDirector.OverlayLayer;
            _objEffectFG.transform.parent = SceneDirector.Instance.transform;
            _objEffectFG.transform.localPosition = new Vector3(0f, 0f, -0.1f);
            meshFilter = _objEffectFG.AddComponent<MeshFilter>();
            meshFilter.mesh = SceneDirector.Instance.CreateQuadMesh();

            _renderTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
            _renderTexture.filterMode = FilterMode.Point;
            _renderTexture.autoGenerateMips = false;
            _renderTexture.Create();

            SceneDirector.Instance.StartCoroutine(CaptureScreen());
        }

        public override void End()
        {
            base.End();

            GameObject.Destroy(_objEffectBG);
            GameObject.Destroy(_objEffectFG);

            foreach (Camera cam in _recordCameras)
            {
                cam.targetTexture = null;
                cam.enabled = true;
                cam.Render();
            }
        }

        public override bool Step(float dt)
        {
            if (!_startRecording)
            {
                _startRecording = true;
                SceneDirector.Instance.StartCoroutine(RecordingScreen());
                return false;
            }

            float f = _easeFadeIn.Step(dt);
            float alpha = Mathf.Lerp(0f, 1f, f);
            Color c = _materialFG.color;
            c.a = alpha;
            _materialFG.color = c;

            return _easeFadeIn.IsFinished;
        }

        IEnumerator CaptureScreen()
        {
            yield return new WaitForEndOfFrame();

            var screenSnapshot = SceneDirector.GetScreenshotTexture();
            _materialBG = _objEffectBG.AddComponent<MeshRenderer>().material;
            _materialBG.shader = Shader.Find("Unlit/Unlit-Texture");
            _materialBG.color = Color.white;
            _materialBG.mainTexture = screenSnapshot;

            _materialFG = _objEffectFG.AddComponent<MeshRenderer>().material;
            _materialFG.shader = Shader.Find("prime[31]/Transitions/Texture With Alpha");
            _materialFG.mainTexture = _renderTexture;
            _materialFG.color = new Color(1f, 1f, 1f, 0f);
        }

        IEnumerator RecordingScreen()
        {
            var allCameras = Camera.allCameras;
            foreach (Camera cam in allCameras)
            {
                if (cam != SceneDirector.Instance.OverlayCamera
                    && cam.enabled
                    && cam.targetTexture == null)
                {
                    cam.enabled = false;
                    cam.targetTexture = _renderTexture;
                    _recordCameras.Add(cam);
                }
            }

            while (!_easeFadeIn.IsFinished)
            {
                yield return new WaitForEndOfFrame();

                foreach (Camera cam in _recordCameras)
                {
                    cam.Render();
                }
            }
        }
    }
}
