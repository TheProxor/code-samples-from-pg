using System;
using System.Collections.Generic;
using DG.Tweening;
using Drawmasters.Levels;
using UnityEngine;


namespace Drawmasters
{
    public static class TimeUtility
    {
        #region Fields

        private static readonly List<object> handlers = new List<object>();

        private static readonly float DefaultTimeScale;
        private static readonly float DefaultFixedStep;

        #endregion



        #region Ctor

        static TimeUtility()
        {
            DefaultTimeScale = Time.timeScale;
            DefaultFixedStep = Time.fixedDeltaTime;
        }

        #endregion



        #region Methods

        public static bool IsAtLeastOneDayPassed(DateTime from, DateTime to) => to.Date.Subtract(from.Date).Days > 0;

        public static void Clear()
        {
            handlers.ForEach(o => DOTween.Kill(o, true));
            handlers.Clear();
            
            Time.timeScale = DefaultTimeScale;
            Time.fixedDeltaTime = DefaultFixedStep;
        }

        
        public static void PlaySlowmoSequence(LevelWinMotionSettings.SlowMotion[] motions,
                                              object id,
                                              Action onComplete)
        {
            Sequence motionSequence = DOTween.Sequence();

            foreach(var motion in motions)
            {
                Tween tween = DOTween.To(() => Time.timeScale,
                                         value =>
                                         {
                                             Time.timeScale = value;
                                             Time.fixedDeltaTime = DefaultFixedStep * value;
                                         },
                                         motion.timeScaleEndValue,
                                         motion.duration)
                                     .SetDelay(motion.delay);
                if (motion.curve != null)
                {
                    tween.SetEase(motion.curve);
                }

                motionSequence.Append(tween);
            }

            motionSequence
                .SetId(id)
                .OnComplete(() => onComplete?.Invoke());

            handlers.Add(id);
        }
        
        #endregion
    }
}

