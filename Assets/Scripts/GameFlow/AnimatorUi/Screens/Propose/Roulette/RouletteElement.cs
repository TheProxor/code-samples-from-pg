using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Drawmasters.Proposal;
using Spine;
using Spine.Unity;
using System;
using Modules.Sound;
using Drawmasters.Advertising;
using Drawmasters.Effects;
using Modules.General;
using TransformMode = Drawmasters.Effects.TransformMode;


namespace Drawmasters.Ui
{
    public class RouletteElement : UiRewardElement
    {
        #region Fields

        private const string FreeReceiveTrigger = "FreeReceive";
        private const string VideoReceiveTrigger = "VideoReceive";
        private const string MoveUpAnimation = "UpAnimation";
        
        [SerializeField] private TMP_Text receivedCurrencyText = default;

        [SerializeField] private Image[] skinImages = default;

        [Header("Animations")]
        [SerializeField] private Animator animator = default;
        [SerializeField] private SkeletonGraphic skeletonGraphics = default;
        [SerializeField] [SpineAnimation(dataField = "skeletonGraphic")] private string openSkeletonAnimation = default;

        [Header("Effects")]
        [SerializeField] private Transform fxReceivedRoot = default;
        [SerializeField] private float fxSuitReceivedDelay = default;
        [SerializeField] private Transform fxSuitReceivedRoot = default;

        #endregion



        #region Properties

        public override string AdsPlacement => AdsVideoPlaceKeys.Roulette;

        public Vector3 RewardIconPosition => fxReceivedRoot.position;

        #endregion



        #region Public methods

        public override void Initialize(RewardData rewardData)
        {
            base.Initialize(rewardData);

            if (RewardData is CurrencyReward currencyReward)
            {
                receivedCurrencyText.text = string.Format(CurrencyFormat, currencyReward.UiRewardText);
            }

            if (RewardData != null)
            {
                Sprite skinSpriteToSet = RewardData.GetUiSprite();

                if (skinSpriteToSet != null)
                {
                    foreach (var skinImage in skinImages)
                    {
                        skinImage.sprite = skinSpriteToSet;
                        skinImage.SetNativeSize();
                    }
                }
            }
        }


        public override void Deinitialize()
        {
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);

            base.Deinitialize();
        }


        public void PlayMoveupAnimation() =>
            animator.SetTrigger(MoveUpAnimation);


        protected override void OnFreeReceived()
        {
            base.OnFreeReceived();

            animator.SetTrigger(FreeReceiveTrigger);

            PlayReceiveAnimation(ref skeletonGraphics);
        }


        protected override void OnReceivedFromVideo()
        {
            base.OnReceivedFromVideo();

            animator.SetTrigger(VideoReceiveTrigger);
            EffectManager.Instance.PlaySystemOnce(EffectKeys.FxGUIButtonBuyClick, parent: fxReceivedRoot, transformMode: TransformMode.Local);

            PlayReceiveAnimation(ref skeletonGraphics);
        }


        private void PlayReceiveAnimation(ref SkeletonGraphic skeleton, Action callback = null)
        {
            EventSystemController.SetSystemEnabled(false, this);

            TrackEntry tracker = skeleton.AnimationState.SetAnimation(default, openSkeletonAnimation, false);
            tracker.Complete += Tracker_Complete;

            SoundManager.Instance.PlayOneShot(AudioKeys.Ui.CHESTOPEN);

            Scheduler.Instance.CallMethodWithDelay(this, () =>
            {
                EffectManager.Instance.PlaySystemOnce(EffectKeys.FxGUISuitcaseOpen, Vector3.zero, Quaternion.identity, fxSuitReceivedRoot, TransformMode.Local);
            }, fxSuitReceivedDelay);

            void Tracker_Complete(TrackEntry trackEntry)
            {
                tracker.Complete -= Tracker_Complete;

                EventSystemController.SetSystemEnabled(true, this);
                callback?.Invoke();
            }
        }

        #endregion
    }
}
