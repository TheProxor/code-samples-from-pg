using Drawmasters.Effects;
using Spine;
using Spine.Unity;
using UnityEngine;
using System;
using Event = Spine.Event;


namespace Drawmasters.Utils
{
    [Serializable]
    public class SkeletonAnimationEffectPlayer
    {
        public Action<SkeletonAnimationEffectPlayer> OnEventHappend;

        [SerializeField] private SkeletonAnimation skeletonAnimation = default;
        [SerializeField] [SpineEvent] private string eventName = default;
        [SerializeField] private Transform fxRoot = default;
        [SerializeField] private bool isLoop = default;

        [SerializeField] [Enum(typeof(EffectKeys))] private string fxKey = default;


        private EffectHandler handler;

        public void Initialize()
        {
            if (skeletonAnimation != null && skeletonAnimation.AnimationState != null)
            {
                skeletonAnimation.AnimationState.Event += AnimationState_Event;
            }
            else
            {
                CustomDebug.Log($"Can't start AnimationEffectPlayer for {nameof(SkeletonAnimation)} {skeletonAnimation}");
            }
        }


        public void Deinitialize()
        {
            if (skeletonAnimation != null && skeletonAnimation.AnimationState != null)
            {
                skeletonAnimation.AnimationState.Event -= AnimationState_Event;
            }

            if (handler != null)
            {
                EffectManager.Instance.ReturnHandlerToPool(handler);
                handler = null;
            }
        }


        public void SetSortingOrders(int value)
        {
            if (handler != null)
            {
                handler.SetSortingOrder(value);
            }
        }


        private void AnimationState_Event(TrackEntry trackEntry, Event e)
        {
            if (e.ToString().Equals(eventName, StringComparison.Ordinal))
            {
                if (isLoop)
                {

                    handler = EffectManager.Instance.CreateSystem(EffectKeys.FxGUIChestShineInside,
                        false,
                        Vector3.zero,
                        Quaternion.identity,
                        fxRoot,
                        Effects.TransformMode.Local);

                    if (handler != null)
                    {
                        handler.transform.localScale = Vector3.one;
                    }
                }
                else
                {
                    EffectManager.Instance.PlaySystemOnce(fxKey, parent: fxRoot, transformMode: Effects.TransformMode.Local);
                }

                OnEventHappend?.Invoke(this);
            }
        }
    }
}
