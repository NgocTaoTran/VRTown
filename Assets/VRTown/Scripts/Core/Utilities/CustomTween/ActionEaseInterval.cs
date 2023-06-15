using UnityEngine;

namespace VRTown.Utilities
{
    public class ActionEaseInterval
    {
        public float duration;
        public TweenFunc.TweenType tweenType;
        public float[] easingParam;

        public bool IsFinished
        {
            get
            {
                return _elapsed >= duration;
            }
        }

        protected float _elapsed;
        protected bool _firstTick = true;

        public void Reset()
        {
            _firstTick = true;
            _elapsed = 0f;
        }

        public float Step(float dt)
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

            float t = Mathf.Max(0f, Mathf.Min(1f, _elapsed / Mathf.Max(duration, Mathf.Epsilon)));
            t = TweenFunc.tweenTo(t, tweenType, easingParam);
            return t;
        }
    }
}
