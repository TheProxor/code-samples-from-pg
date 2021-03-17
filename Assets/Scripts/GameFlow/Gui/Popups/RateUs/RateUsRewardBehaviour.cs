using System;
using Drawmasters.Effects;
using Drawmasters.Helpers;
using Drawmasters.ServiceUtil.Interfaces;
using Modules.General;
using Modules.Sound;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Drawmasters.Ui
{
    // TODO; just temp logic for build. have to find out good usage after getting final mockups and visuals
    public class RateUsRewardBehaviour : IUiBehaviour
    {
        #region Nested types

        [Serializable]
        public class Data
        {
            [Header("Reward")]
            public GameObject rewardRoot = default;
            public TMP_Text[] rewardFormatTexts = default;
            public Button receiveRewardButton = default;

            [Header("Texts")]
            public TMP_Text descriptionText = default;

            [Header("Rewarded")]
            public Transform fxRateFlashRoot = default;
            public IdleEffect fxRateIdle = default;

            [Header("Loader")]
            public float loaderButtonShowDuration = default;
            public GameObject loaderButton = default;

            [Header("Animations")]
            public SpineAnimationSequencePlayer.Data sequenceAnimationData = default;
            public float animationDelay = default;
        }

        #endregion



        #region Fields

        private readonly Data data;
        private readonly RateUsFeedbackPopupScreen animatorScreen;
        private readonly IRateUsService rateUsService;

        #endregion



        #region Class lifecycle

        public RateUsRewardBehaviour(Data _data, RateUsFeedbackPopupScreen _animatorScreen, IRateUsService _rateUsService)
        {
            data = _data;
            animatorScreen = _animatorScreen;
            rateUsService = _rateUsService;
        }

        #endregion



        #region IUiBehaviour

        public void Enable()
        {
            data.fxRateIdle.CreateAndPlayEffect();

            data.descriptionText.text = rateUsService.GetDescriptionText(false);

            CommonUtility.SetObjectActive(data.receiveRewardButton.gameObject, false);
            CommonUtility.SetObjectActive(data.rewardRoot, rateUsService.IsRewardAvailable);

            PlayAppearAnimation(data.animationDelay);

            foreach (var rewardFormatText in data.rewardFormatTexts)
            {
                rewardFormatText.text = rateUsService.UiRewardText;
            }

            animatorScreen.OnRated += AnimatorScreen_OnRated;
            animatorScreen.OnShouldPlayFxs += AnimatorScreen_OnShouldPlayFxs;
        }


        public void Disable()
        {
            animatorScreen.OnRated -= AnimatorScreen_OnRated;
            animatorScreen.OnShouldPlayFxs -= AnimatorScreen_OnShouldPlayFxs;

            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
            data.fxRateIdle.StopEffect();
        }


        public void InitializeButtons()
        {
            data.receiveRewardButton.onClick.AddListener(ReceiveRewardButton_OnClick);
        }


        public void DeinitializeButtons()
        {
            data.receiveRewardButton.onClick.RemoveListener(ReceiveRewardButton_OnClick);
        }


        public void Deinitialize() =>
            Disable();

        #endregion



        #region Methods

        private void PlayAppearAnimation(float delay = 0.0f)
        {
            CommonUtility.SetObjectActive(data.sequenceAnimationData.skeletonGraphic.gameObject, false);
            Scheduler.Instance.CallMethodWithDelay(this, PlayAnimationAction, delay);


            void PlayAnimationAction()
            {
                CommonUtility.SetObjectActive(data.sequenceAnimationData.skeletonGraphic.gameObject, true);

                SpineAnimationSequencePlayer player = new SpineAnimationSequencePlayer();
                player.Play(data.sequenceAnimationData, shouldLoopEnd: true);
            }
        }

        #endregion



        #region Events handlers

        private void ReceiveRewardButton_OnClick()
        {
            rateUsService.ApplyReward();
            animatorScreen.Hide();
        }


        private void AnimatorScreen_OnRated()
        {
            data.descriptionText.text = rateUsService.GetDescriptionText(true);

            if (rateUsService.IsRewardAvailable)
            {
                data.receiveRewardButton.gameObject.SetActive(true);
            }
            else
            {
                animatorScreen.Hide();
            }
        }


        private void AnimatorScreen_OnShouldPlayFxs()
        {
            EffectManager.Instance.PlaySystemOnce(EffectKeys.FxGUIRateUsFlash, Vector3.zero, Quaternion.identity, data.fxRateFlashRoot, TransformMode.Local);
            SoundManager.Instance.PlayOneShot(AudioKeys.Ui.LIVEOPS_MAP_UNLOCK);
        }

        #endregion
    }
}
