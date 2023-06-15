using UnityEngine;
using System.Collections;

namespace VRTown.Scene
{
    public class TransitionBlur : SceneTransitionConcurrent
    {
        public float duration;
        public float blurMin = 0.0f;
        public float blurMax = 0.01f;

        private GameObject _objEffect;
        private Material _material;

        private bool _firstTick = true;
        private float _elapsed;

        public override void Enter()
        {
            base.Enter();

            // create an overlay object
            _objEffect = new GameObject("TransitionBlur");
            _objEffect.layer = SceneDirector.OverlayLayer;
            _objEffect.transform.parent = SceneDirector.Instance.transform;
            var meshFilter = _objEffect.AddComponent<MeshFilter>();
            meshFilter.mesh = SceneDirector.Instance.CreateQuadMesh();

            SceneDirector.Instance.StartCoroutine(CaptureScreen());
        }

        public override void End()
        {
            base.End();
            GameObject.Destroy(_objEffect);
        }

        public override bool Step(float dt)
        {
            if (_firstTick)
            {
                _firstTick = false;
                _elapsed = 0;
            }
            else
            {
                _elapsed += dt;
            }

            var f = Mathf.Pow(_elapsed / duration, 2f);
            var blurAmount = Mathf.Lerp(blurMin, blurMax, f);
            _material.SetFloat("_BlurSize", blurAmount);

            return _elapsed >= duration;
        }

        IEnumerator CaptureScreen()
        {
            yield return new WaitForEndOfFrame();

            var screenSnapshot = SceneDirector.GetScreenshotTexture();
            _material = _objEffect.AddComponent<MeshRenderer>().material;
            _material.shader = Shader.Find("prime[31]/Transitions/Blur");
            _material.color = Color.white;
            _material.mainTexture = screenSnapshot;
            _material.SetFloat("_BlurSize", 0f);
        }
    }

}