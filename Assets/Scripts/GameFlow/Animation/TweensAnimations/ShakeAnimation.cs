using System;
using DG.Tweening;
using Drawmasters.Levels;
using UnityEngine;
using Object = System.Object;


namespace Drawmasters
{
    [Serializable]
    public class ShakeAnimation
    {
        #region Fields

        public CameraShakeSettings.Shake settings = default;

        #endregion



        #region Public methods

        public void Play(Transform shakeTransform,
                               object shakeId,
                               Action callback = null)
        {
            shakeTransform
                .DOShakePosition(settings.duration, settings.strength, settings.vibrato, settings.randomness, true, settings.isFadeOut)
                .SetDelay(settings.delay)
                .OnComplete(() => callback?.Invoke())
                .SetId(shakeId);
        }


        public void Stop(Object shakeId) =>
                DOTween.Complete(shakeId, true);

        #endregion
    }
}
