using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;


namespace Drawmasters
{
    [Serializable]
    public class CommonAnimation
    {
        #region Fields

        [MinValue(0.0f)]
        public float duration = default;

        public AnimationCurve curve = default;

        [MinValue(0.0f)]
        public float delay = default;

        public bool loop = default;
        [ShowIf("loop")] public LoopType loopType = default;

        public bool shouldUseUnscaledDeltaTime = default;

        #endregion



        #region Properties

        public virtual float Time => duration + delay;

        #endregion



        #region Class lifecycle

        public CommonAnimation()
        {
            curve = new AnimationCurve(new Keyframe(0.0f, 0.0f, 0.0f, 0.0f),
                                       new Keyframe(1.0f, 1.0f, 0.0f, 0.0f));
        }

        #endregion



        #region Public methods

        public void SetupData(CommonAnimation animation)
        {
            duration = animation.duration;
            curve = animation.curve;
            delay = animation.delay;
            shouldUseUnscaledDeltaTime = animation.shouldUseUnscaledDeltaTime;
        }

        #endregion
    }
}
