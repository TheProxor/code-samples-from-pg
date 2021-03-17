using System;
using Drawmasters.Effects;
using DG.Tweening;
using Modules.General;
using UnityEngine;


namespace Drawmasters.Utils
{
    public class TrailAnimationHelper : IInitializable, IDeinitializable
    {
        #region Fields

        private readonly string fxKey;
        private readonly VectorAnimation trailAnimation;
        private readonly Transform trailRoot;

        #endregion



        #region Class lifecycle

        public TrailAnimationHelper(VectorAnimation _trailAnimation, string _fxKey, Transform _trailRoot)
        {
            trailAnimation = _trailAnimation;
            fxKey = _fxKey;
            trailRoot = _trailRoot;
        }

        #endregion



        #region Public methods

        public void Initialize()
        {
        }


        public void Deinitialize()
        {
            DOTween.Kill(this);
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
        }


        public virtual void PlayTrail(Vector3 startPosition, Vector3 endPosition, Action callback = null, float additionalTrailHideDelay = 1.0f)
        {
            var handler = EffectManager.Instance.CreateSystem(fxKey, true, startPosition, parent: trailRoot);

            float callbackDelay = trailAnimation.Time;

            if (handler != null)
            {
                Scheduler.Instance.CallMethodWithDelay(this, () => EffectManager.Instance.ReturnHandlerToPool(handler), callbackDelay + additionalTrailHideDelay);

                handler.transform.position = startPosition;
                handler.transform.localScale = Vector3.one;
                handler.Play();

                trailAnimation.SetupBeginValue(startPosition);
                trailAnimation.SetupEndValue(endPosition);

                trailAnimation.Play((value) => handler.transform.position = value, this, () => callback?.Invoke());
            }
            else
            {
                Scheduler.Instance.CallMethodWithDelay(this, () => callback?.Invoke(), callbackDelay);
            }
        }

        #endregion
    }
}
