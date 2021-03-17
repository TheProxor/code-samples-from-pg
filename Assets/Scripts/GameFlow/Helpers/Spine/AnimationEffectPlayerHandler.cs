using Drawmasters.Effects;
using Spine;
using Spine.Unity;
using UnityEngine;
using System;
using Event = Spine.Event;


namespace Drawmasters.Utils
{
    public class AnimationEffectPlayerHandler : AnimationActionHandler
    {
        #region Fields

        private readonly string fxBoneName;
        private readonly string fxKey;
        private string stopEventName;
        private bool shouldAttachToTransform = true;

        private EffectHandler effectHandler;
        private GameObject fxRoot;

        #endregion



        #region Class lifecycle

        public AnimationEffectPlayerHandler(SkeletonGraphic _skeletonGraphic, string _eventName, string _fxBoneName, string _fxKey)
            : base(_skeletonGraphic, _eventName)
        {
            fxBoneName = _fxBoneName;
            fxKey = _fxKey;
        }

        #endregion



        #region Methods

        public override void Initialize()
        {
            base.Initialize();

            if (skeletonGraphic != null)
            {
                if (fxRoot != null)
                {
                    Content.Management.DestroyObject(fxRoot);
                    fxRoot = null;
                }

                fxRoot = SpineUtility.InstantiateBoneFollower(skeletonGraphic, fxBoneName, skeletonGraphic.transform);
            }
        }


        public override void Deinitialize()
        {
            EffectManager.Instance.ReturnHandlerToPool(effectHandler);

            if (fxRoot != null)
            {
                Content.Management.DestroyObject(fxRoot);
                fxRoot = null;
            }

            base.Deinitialize();
        }


        public void SetStopFxEvent(string _stopEventName) =>
            stopEventName = _stopEventName;


        public void SetAttachToRoot(bool value) =>
            shouldAttachToTransform = value;

        #endregion



        #region Events handlers

        protected override void OnMonitorEventHappened()
        {
            base.OnMonitorEventHappened();

            Vector3 worldRotation = Vector3.zero;

            if (fxRoot.transform.rotation.z < 90)
            {
                worldRotation.z += 180.0f;
            }

            EffectManager.Instance.ReturnHandlerToPool(effectHandler);

            Transform parent = shouldAttachToTransform ? fxRoot.transform : null;
            effectHandler = EffectManager.Instance.PlaySystemOnce(fxKey, fxRoot.transform.position, Quaternion.Euler(worldRotation), parent);
        }


        protected override void AnimationState_Event(TrackEntry trackEntry, Event e)
        {
            base.AnimationState_Event(trackEntry, e);

            if (e.ToString().Equals(stopEventName, StringComparison.Ordinal))
            {
                effectHandler.Stop();
            }
        }

        #endregion
    }
}
