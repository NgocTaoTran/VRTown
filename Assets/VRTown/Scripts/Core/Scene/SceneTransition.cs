using UnityEngine;
using System.Collections;

namespace VRTown.Scene
{
    public interface TransitionDelegate
    {
        IEnumerator Execute();
    }

    public class SceneTransitionSequential : TransitionDelegate
    {
        public SceneLoader sceneLoader;

        public System.Action onStepOutEnter;
        public System.Action onStepOutDidFinish;
        public System.Action onStepInEnter;
        public System.Action onStepInDidFinish;

        public IEnumerator Execute()
        {
            Enter();

            if (sceneLoader != null)
            {
                sceneLoader.PreLoad();
            }

            if (onStepOutEnter != null)
            {
                onStepInEnter();
            }

            while (!StepOut(Time.deltaTime))
                yield return null;

            if (onStepOutDidFinish != null)
            {
                onStepOutDidFinish();
            }

            if (sceneLoader != null)
            {
                sceneLoader.PreLoadDone();
                sceneLoader.Load();
            }

            if (sceneLoader != null)
            {
                while (!sceneLoader.IsFinished())
                {
                    StepLoad(Time.deltaTime);
                    yield return null;
                }

                sceneLoader.ActiveScene();
            }

            if (onStepInEnter != null)
            {
                onStepInEnter();
            }

            while (!StepIn(Time.deltaTime))
                yield return null;

            if (onStepInDidFinish != null)
            {
                onStepInDidFinish();
            }

            End();
        }

        public virtual void StepLoad(float dt)
        {

        }

        public virtual bool StepOut(float dt)
        {
            return true;
        }

        public virtual bool StepIn(float dt)
        {
            return true;
        }

        public virtual void Enter()
        {

        }

        public virtual void End()
        {

        }
    }

    public class SceneTransitionConcurrent : TransitionDelegate
    {
        public SceneLoader sceneLoader;

        public IEnumerator Execute()
        {
            Enter();

            if (sceneLoader != null)
            {
                sceneLoader.Load();

                yield return null;

                sceneLoader.ActiveScene();

                while (!sceneLoader.IsFinished())
                {
                    StepLoad(Time.deltaTime);
                    yield return null;
                }
            }

            while (!Step(Time.deltaTime))
                yield return null;

            End();
        }

        public virtual void StepLoad(float dt)
        {

        }

        public virtual bool Step(float dt)
        {
            return true;
        }

        public virtual void Enter()
        {

        }

        public virtual void End()
        {

        }
    }
}