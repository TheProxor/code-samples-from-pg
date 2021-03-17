using System;
using UnityEngine;
using Drawmasters.Helpers;
using Drawmasters.Effects;
using TMPro;
using Spine.Unity;
using Modules.General;
using Modules.Sound;


namespace Drawmasters.Proposal.Ui
{
    public class UiHappyHoursEvent : UiLiveOpsEvent
    {
        #region Fields

        private const string MultiplierTextFormat = "x{0}";

        [SerializeField] private SpineAnimationSequencePlayer.Data sequenceAnimationData = default;
        [SerializeField] private TMP_Text multiplierText = default;
        [SerializeField] private BoneFollowerGraphic[] boneFollowerGraphics = default;
        [SerializeField] private float animationDelay = default;

        [Header("Force propose")]
        [SerializeField] private Transform fxForceProposeRoot = default;
        [SerializeField] private float forceProposeAnimationDelay = default;
        [SerializeField] private float forceProposeFxsDelay = default;

        #endregion



        #region Methods

        public override void Initialize(LiveOpsEventController _controller)
        {
            base.Initialize(_controller);

            PlayAppearAnimation(animationDelay);

            multiplierText.text = string.Format(MultiplierTextFormat, _controller.PointsMultiplier.RoundToUiView());
        }


        public override void Deinitialize()
        {
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);

            base.Deinitialize();
        }


        protected override void OnForcePropose()
        {
            base.OnForcePropose();

            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
            PlayAppearAnimation(forceProposeAnimationDelay, PlayForceProposeFx);


            void PlayForceProposeFx()
            {
                Scheduler.Instance.CallMethodWithDelay(this, () =>
                {
                    EffectManager.Instance.PlaySystemOnce(EffectKeys.FxGUIHappyHoursForcePropose, 
                        fxForceProposeRoot.position, 
                        fxForceProposeRoot.rotation, 
                        fxForceProposeRoot);
                    
                    SoundManager.Instance.PlayOneShot(AudioKeys.Ui.SKULL_ADD);
                }, forceProposeFxsDelay);
            }
        }


        private void PlayAppearAnimation(float delay = 0.0f, Action callback = default)
        {
            CommonUtility.SetObjectActive(sequenceAnimationData.skeletonGraphic.gameObject, false);
            Scheduler.Instance.CallMethodWithDelay(this, PlayAnimationAction, delay);


            void PlayAnimationAction()
            {
                CommonUtility.SetObjectActive(sequenceAnimationData.skeletonGraphic.gameObject, true);

                SpineAnimationSequencePlayer player = new SpineAnimationSequencePlayer();
                player.Play(sequenceAnimationData, shouldLoopEnd: true);

                foreach (var boneFollowerGraphic in boneFollowerGraphics)
                {
                    boneFollowerGraphic.Initialize();
                }

                callback?.Invoke();
            }
        }

        #endregion
    }
}
