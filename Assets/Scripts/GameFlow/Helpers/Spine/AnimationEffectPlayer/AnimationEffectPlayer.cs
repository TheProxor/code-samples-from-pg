using Drawmasters.Effects;
using Spine;
using Spine.Unity;
using UnityEngine;
using System;
using Event = Spine.Event;


namespace Drawmasters.Utils
{
    // TODO: rename on SkeletonGraphicEffectPlayer.cs to avoid misunderstanding with SkeletonAnimationEffectPlayer.cs
    [Serializable]
    public class AnimationEffectPlayer
    {
        public Action<AnimationEffectPlayer> OnEventHappend;

        [SerializeField] private SkeletonGraphic skeletonGraphic = default; 
        [SerializeField] [SpineEvent] private string eventName = default;
        [SerializeField] private Transform fxRoot = default;
        [SerializeField] private bool isLoop = default;
        
        [SerializeField] [Enum(typeof(EffectKeys))] private string fxKey = default;


        private EffectHandler handler;

        public void Initialize()
        {
            if (skeletonGraphic != null && skeletonGraphic.AnimationState != null)
            {
                skeletonGraphic.AnimationState.Event += AnimationState_Event;
            }
            else
            {
                CustomDebug.Log($"Can't start AnimationEffectPlayer for skeletonGraphic {skeletonGraphic}");
            }
        }


        public void Deinitialize()
        {
            if (skeletonGraphic != null && skeletonGraphic.AnimationState != null)
            {
                skeletonGraphic.AnimationState.Event -= AnimationState_Event;
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
