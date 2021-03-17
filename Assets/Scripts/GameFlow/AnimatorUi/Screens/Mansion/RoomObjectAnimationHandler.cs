using Spine.Unity;
using System;
using System.Collections.Generic;
using Modules.General;
using UnityEngine;
using Drawmasters.Tutorial;
using Drawmasters.Utils;
using Drawmasters.Ui;
using Drawmasters.Mansion;


namespace Drawmasters.Ui.Mansion
{
    public class RoomObjectAnimationHandler : MonoBehaviour, IInitializable
    {
        #region Helper types

        [Serializable]
        private class AnimationInfo
        {
            public SkeletonGraphic skeletonGraphic = default;
            [SpineAnimation(dataField = "skeletonGraphic")]
            public string animationName = default;

            public float delay = default;


            public GameObject CompletedRoot => skeletonGraphic.gameObject;
        }
        #endregion



        #region Constants

        //hammer animation keys
        private const string AcceptBoune = "AcceptBounce";

        private const string ErrorBounce = "ErrorBounce";

        private const string HideHammerTrigger = "Hide";

        private const string HiddenHammer = "Hidden";

        private const string Show = "Show";

        private const string Disabled = "Disabled";

        //objects animation keys
        private const string ShowObjects = "Show";

        private const string ShownObjects = "Shown";


        #endregion



        #region Fields

        public HammerControl hammerControl = default;

        [SerializeField] private Animator objectsAnimator = default;

        [SerializeField] private List<AnimationInfo> showAnimationInfo = default;
        [SerializeField] private List<AnimationInfo> idleAnimationInfo = default;
        [SerializeField] private GameObject[] disableOnComplteteObjects = default;

        #endregion



        #region Public methods

        public void Initialize()
        {
        }


        public void Deinitialize()
        {
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
        }


        public void PlayUpgradeAnimation()
        {
            hammerControl.hammerAnimator.SetTrigger(AcceptBoune);
        }


        //TODO bad naming
        public void PlayNonUpgradeAnimation()
        {
            hammerControl.hammerAnimator.SetTrigger(ErrorBounce);
        }


        public void HideHammer()
        {
            hammerControl.hammerAnimator.SetTrigger(HideHammerTrigger);
            hammerControl.hammerAnimator.ResetTrigger(Show);

            hammerControl.hammerButton.enabled = false;

            HideAnimations();
        }


        public void SetHammerHidden()
        {
            hammerControl.hammerAnimator.SetTrigger(HiddenHammer);
            hammerControl.hammerAnimator.ResetTrigger(Show);

            hammerControl.hammerButton.enabled = false;

            HideAnimations();
        }


        public void ShowHammer()
        {
            hammerControl.hammerAnimator.SetTrigger(Show);

            hammerControl.hammerButton.enabled = true;

            HideAnimations();
        }


        public void SetDisabledState()
        {
            hammerControl.hammerAnimator.SetTrigger(Disabled);

            hammerControl.hammerButton.enabled = false;

            HideAnimations();
        }

        public void PlayShowObjectsAnimation()
        {
            objectsAnimator.SetTrigger(Show);

            foreach (var i in showAnimationInfo)
            {
                Scheduler.Instance.CallMethodWithDelay(this, () =>
                {
                    CommonUtility.SetObjectActive(i.CompletedRoot, true);
                    i.skeletonGraphic.enabled = true;

                    SpineUtility.SafeSetAnimation(i.skeletonGraphic, i.animationName);

                    var info = idleAnimationInfo.Find(t => t.skeletonGraphic == i.skeletonGraphic);

                    if (info != null)
                    {
                        SpineUtility.SafeAddAnimation(i.skeletonGraphic, info.animationName, 0, true);
                    }
                }, i.delay);
            }
        }


        public void SetObjectsShownState()
        {
            objectsAnimator.SetTrigger(ShownObjects);

            foreach (var i in idleAnimationInfo)
            {
                CommonUtility.SetObjectActive(i.CompletedRoot, true);
                i.skeletonGraphic.enabled = true;

                if (i.skeletonGraphic.AnimationState != null)
                {
                    i.skeletonGraphic.AnimationState.SetAnimation(0, i.animationName, true);
                }
                else
                {
                    CustomDebug.Log($"Skeleton asset {i.skeletonGraphic} corrupted!");
                }
            }
        }


        public void ApplyRoomCompleteState()
        {
            foreach (var go in disableOnComplteteObjects)
            {
                go.SetActive(false);
            }
        }

        #endregion


        #region Private methods

        private void HideAnimations()
        {
            foreach (var i in idleAnimationInfo)
            {
                CommonUtility.SetObjectActive(i.CompletedRoot, false);
                i.skeletonGraphic.enabled = false;
            }
        }

        #endregion
    }
}

