using System;
using DG.Tweening;
using DG.Tweening.Core;


namespace Drawmasters
{
    [Serializable]
    public class NumberAnimation : CommonAnimation
    {
        #region Fields

        public float beginValue = default;
        public float endValue = default;

        #endregion



        #region Public methods

        public Tween Play(DOSetter<float> setter, object handler, Action callback = null)
        {
           
            var tween = DOTween
                .To(() => beginValue, setter, endValue, duration)
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


        public void SetupBeginValue(float value) =>
           beginValue = value;


        public void SetupEndValue(float value) =>
            endValue = value;


        public void SetupData(FactorAnimation animation)
        {
            base.SetupData(animation);

            beginValue = animation.beginValue;
            endValue = animation.endValue;
        }

        #endregion
    }
}
