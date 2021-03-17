using System;
using DG.Tweening;
using DG.Tweening.Core;
using UnityEngine;


namespace Drawmasters
{
    [Serializable]
    public class VectorAnimation : CommonAnimation
    {
        #region Fields

        public Vector3 beginValue = default;
        public Vector3 endValue = default;

        #endregion



        #region Public methods

        public Tween Play(DOSetter<Vector3> setter, object handler, Action callback = null, bool isReversed = false)
        {
            Vector3 begin = isReversed ? endValue : beginValue;
            Vector3 end = isReversed ? beginValue : endValue;

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


        public void SetupDelay(float value) =>
            delay = value;

        public void SetupBeginValue(Vector3 value) =>
            beginValue = value;


        public void SetupEndValue(Vector3 value) =>
            endValue = value;


        public void SetupDuration(float value) =>
            duration = value;


        public void SetupData(VectorAnimation animation)
        {
            base.SetupData(animation);

            beginValue = animation.beginValue;
            endValue = animation.endValue;
        }

        #endregion
    }
}
