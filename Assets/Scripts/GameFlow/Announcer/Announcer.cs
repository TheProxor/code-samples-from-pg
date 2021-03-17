using System;
using UnityEngine;

namespace Drawmasters.Announcer
{
    public abstract class Announcer : IDeinitializable
    {
        #region Fields

        protected readonly Transform animatedObject;
        protected readonly VectorAnimation animation;
        protected abstract VectorAnimation AnnouncerAnimation { get; }
        #endregion


        
        #region Ctor

        protected Announcer(Transform animatable)
        {
            animatedObject = animatable;
            animation = AnnouncerAnimation;

        }

        #endregion


        
        #region Methods

        public event Action<Announcer> OnReady;

        
        public virtual void Show()
        {
            CommonUtility.SetObjectActive(animatedObject.gameObject, true);
            animation.Play(scale => animatedObject.localScale = scale, this, () => Hide());
        }
        
        
        public virtual void Hide(bool isImmediately = false)
        {
            animatedObject.localScale = animation.beginValue;
            CommonUtility.SetObjectActive(animatedObject.gameObject, false);
        }

        
        protected void Ready(Announcer announcer)
        {
            OnReady?.Invoke(announcer);
        }
        
        #endregion
        
        
        
        #region IDeinitializable
        
        public abstract void Deinitialize();
        
        #endregion
    }
}