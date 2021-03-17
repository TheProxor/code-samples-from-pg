using System;
using DG.Tweening;
using DG.Tweening.Core;
using UnityEngine;


namespace Drawmasters
{
    [Serializable]
    public class ColorAnimation : CommonAnimation
    {
        #region Fields

        public Color beginValue = Color.white;
        public Color endValue = Color.white;

        #endregion



        #region Methods

        public void Play(DOSetter<Color> setter, object handler, Action callback = null, bool isReversed = false)
        {
            Color begin = isReversed ? endValue : beginValue;
            Color end = isReversed ? beginValue : endValue;

            var tween = DOTween
                 .To(() => begin, setter, end, duration)
                 .SetDelay(delay)
                 .SetEase(curve)
                 .SetUpdate(shouldUseUnscaledDeltaTime)
                 .SetId(handler)
                 .OnComplete(() => callback?.Invoke());

            if (loop)
            {
                tween.SetLoops(-1, loopType);
            }
        }


        public void SetupData(ColorAnimation animation)
        {
            base.SetupData(animation);

            beginValue = animation.beginValue;
            endValue = animation.endValue;
        }


        public void SetupBeginValue(Color value) =>
            beginValue = value;


        public void SetupEndValue(Color value) =>
            endValue = value;


        public void SetupDuration(float value) =>
            duration = value;

        #endregion
    }
}
