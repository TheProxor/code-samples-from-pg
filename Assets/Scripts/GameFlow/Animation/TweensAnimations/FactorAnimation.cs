using System;
using DG.Tweening;
using DG.Tweening.Core;
using UnityEngine;


namespace Drawmasters
{
    [Serializable]
    public class FactorAnimation : CommonAnimation
    {
        #region Fields

        [Range(0.0f, 1.0f)]
        public float beginValue = default;

        [Range(0.0f, 1.0f)]
        public float endValue = default;

        #endregion



        #region Public methods

        public Tween Play(DOSetter<float> setter, object handler, Action callback = null, bool isReversed = false)
        {
            float begin = isReversed ? endValue : beginValue;
            float end = isReversed ? beginValue : endValue;

            var tween = DOTween
                .To(() => begin, setter, end, duration)
                .SetDelay(delay)
                .SetEase(curve)
                .SetId(handler)
                .SetUpdate(shouldUseUnscaledDeltaTime)
                .OnComplete(() => callback?.Invoke());

            if (loop)
            {
                tween.SetLoops(-1, loopType);
            }

            return tween;
        }

        public void SetDelay(float value) =>
            delay = value;


        public void SetupData(FactorAnimation animation)
        {
            base.SetupData(animation);

            beginValue = animation.beginValue;
            endValue = animation.endValue;
        }

        #endregion
    }
}
