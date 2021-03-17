using Spine;
using Spine.Unity;
using System;
using UnityEngine.Events;
using Event = Spine.Event;


namespace Drawmasters.Utils
{
    public class AnimationActionHandler
    {
        #region Fields

        protected readonly SkeletonGraphic skeletonGraphic;
        protected readonly string monitoredEventName;

        public UnityEvent OnEventHappened { get; private set; } = new UnityEvent();

        #endregion



        #region Class lifecycle

        public AnimationActionHandler(SkeletonGraphic _skeletonGraphic, string _eventName)
        {
            skeletonGraphic = _skeletonGraphic;
            monitoredEventName = _eventName;
        }

        #endregion



        #region Methods

        public virtual void Initialize()
        {
            if (skeletonGraphic != null && skeletonGraphic.AnimationState != null)
            {
                skeletonGraphic.AnimationState.Event += AnimationState_Event;
            }
            else
            {
                CustomDebug.Log($"Can't start {this} for skeletonGraphic {skeletonGraphic}");
            }
        }


        public virtual void Deinitialize()
        {
            if (skeletonGraphic != null && skeletonGraphic.AnimationState != null)
            {
                skeletonGraphic.AnimationState.Event -= AnimationState_Event;
            }

            OnEventHappened.RemoveAllListeners();
        }


        protected virtual void OnMonitorEventHappened() =>
            OnEventHappened?.Invoke();

        #endregion



        #region Events handlers

        protected virtual void AnimationState_Event(TrackEntry trackEntry, Event e)
        {
            if (e.ToString().Equals(monitoredEventName, StringComparison.Ordinal))
            {
                OnMonitorEventHappened();
            }
        }

        #endregion
    }
}
