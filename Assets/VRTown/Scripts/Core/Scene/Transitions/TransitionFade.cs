using VRTown.Utilities;
using UnityEngine;

namespace VRTown.Scene
{
    public class TransitionFade : SceneTransitionSequential
    {
        public float duration;
        public Color color;
        public TweenFunc.TweenType tweenOut;
        public TweenFunc.TweenType tweenIn;

        private ActionEaseInterval _easeFadeOut;
        private ActionEaseInterval _easeFadeIn;

        private GameObject _objEffect;
        private Material _material;

        public override void Enter()
        {
            base.Enter();

            _easeFadeOut = new ActionEaseInterval()
            {
                duration = duration * 0.5f,
                tweenType = tweenOut,
                easingParam = null
            };

            _easeFadeIn = new ActionEaseInterval()
            {
                duration = duration * 0.5f,
                tweenType = tweenIn,
                easingParam = null
            };

            // create an overlay object
            _objEffect = new GameObject("TransitionFade");
            _objEffect.layer = SceneDirector.OverlayLayer;
            _objEffect.transform.parent = SceneDirector.Instance.transform;
            _objEffect.transform.localScale = Vector2.one * 2;
            var meshFilter = _objEffect.AddComponent<MeshFilter>();
            meshFilter.mesh = SceneDirector.Instance.CreateQuadMesh();
            _material = _objEffect.AddComponent<MeshRenderer>().material;
            _material.shader = Shader.Find("Custom/ScreenColorOverlay");
            _material.color = color;
        }

        public override void End()
        {
            base.End();
            GameObject.Destroy(_objEffect);
        }

        public override bool StepOut(float dt)
        {
            float f = _easeFadeOut.Step(dt);
            float alpha = Mathf.Lerp(0f, 1f, f);
            Color c = _material.color;
            c.a = alpha;
            _material.color = c;

            return _easeFadeOut.IsFinished;
        }

        public override bool StepIn(float dt)
        {
            float f = _easeFadeIn.Step(dt);
            float alpha = Mathf.Lerp(1f, 0f, f);
            Color c = _material.color;
            c.a = alpha;
            _material.color = c;

            return _easeFadeIn.IsFinished;
        }
    }
}

