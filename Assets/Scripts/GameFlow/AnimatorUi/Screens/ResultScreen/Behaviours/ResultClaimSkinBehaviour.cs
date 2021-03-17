using System;
using System.Collections.Generic;
using Drawmasters.Advertising;
using Drawmasters.Utils;
using Spine.Unity;
using UnityEngine;
using Drawmasters.Effects;
using Drawmasters.Levels;
using Drawmasters.ServiceUtil;
using Modules.Analytics;
using Modules.General;
using Modules.General.Abstraction;
using Modules.Sound;
using Sirenix.OdinInspector;
using UnityEngine.UI;

namespace Drawmasters.Ui
{
    public class ResultClaimSkinBehaviour : IResultBehaviour
    {
        #region Helpers

        [Serializable]
        public class Data
        {
            [Required] public GameObject rootObject = default;
            [Required] public GameObject headerRootObject = default;
            [Required] public SkeletonGraphic animation = default;
            [Required] public IdleEffect openSkinVfx = default;
            [Required] public RewardedVideoButton getSkinButton = default;
            [Required] public Button skipSkinButton = default;
            public float delayAfterClaim = default;
        }

        #endregion



        #region Fields

        private readonly Data data;
        private readonly ResultScreen resultScreen;

        private readonly List<GameObject> objects;

        #endregion



        #region Ctor

        public ResultClaimSkinBehaviour(Data _data, ResultScreen screen)
        {
            data = _data;
            resultScreen = screen;

            objects = new List<GameObject> { data.rootObject, data.headerRootObject };
        }

        #endregion



        #region IDeinitializable

        public void Deinitialize() => EndHandleHideModeActionEvents();

        #endregion



        #region IResultBehaviour

        public void Enable()
        {
            objects.ForEach(go => CommonUtility.SetObjectActive(go, true));

            ShooterSkinType skinType = GameServices.Instance.ProposalService.SkinProposal.CurrentSkinTypeToPropose;

            data.animation.skeletonDataAsset = IngameData.Settings.shooterSkinsSettings.GetSkeletonDataAsset(skinType);
            data.animation.initialSkinName = string.Empty;
            data.animation.Initialize(true);

            SpineUtility.SetShooterSkin(skinType, data.animation);

            PlayReceivedAnimation();

            SoundManager.Instance.PlayOneShot(AudioKeys.Ui.SKULL_ADD);

            data.getSkinButton.Initialize(AdsVideoPlaceKeys.ClaimSkin);
            data.getSkinButton.OnVideoShowEnded += ClaimSkinButton_OnVideoShowEnded;
            data.getSkinButton.InitializeButtons();

            data.skipSkinButton.onClick.AddListener(SkipSkinButton_OnClick);
        }

        public void Disable()
        {
            EndHandleHideModeActionEvents();

            objects.ForEach(go => CommonUtility.SetObjectActive(go, false));
            data.openSkinVfx.StopEffect();

            DeinitializeButtons();
        }

        private void PlayReceivedAnimation()
        {
            ShooterAnimationSettings animationSettings = IngameData.Settings.shooterAnimationSettings;
            data.animation.AnimationState.SetAnimation(0, animationSettings.RandomDanceAnimation, false);
            data.animation.AnimationState.AddAnimation(0, ShooterAnimation.IdleEmptyAnimation, true, 0f);

            data.openSkinVfx.CreateAndPlayEffect();
        }

        public void InitializeButtons() { }

        public void DeinitializeButtons()
        {
            data.getSkinButton.OnVideoShowEnded -= ClaimSkinButton_OnVideoShowEnded;
            data.getSkinButton.Deinitialize();

            data.skipSkinButton.onClick.RemoveListener(SkipSkinButton_OnClick);
        }

        #endregion



        #region Events handlers

        private void ClaimSkinButton_OnVideoShowEnded(AdActionResultType result)
        {
            if (result == AdActionResultType.Success)
            {
                ShooterSkinType skin = GameServices.Instance.ProposalService.SkinProposal.CurrentSkinTypeToPropose;

                if (skin != ShooterSkinType.None)
                {
                    CommonEvents.SendAdVideoReward(data.getSkinButton.Placement);

                    GameServices.Instance.ProposalService.SkinProposal.ClaimCurrentSkin();

                    LevelProgressObserver.TriggerClaimSkin();

                    PlayReceivedAnimation();

                    SetButtonsEnabled(false);

                    BeginHandleHideModeActionEvents();
                }
                else
                {
                    LoadModeHideAction();
                }
            }

            void SetButtonsEnabled(bool enabled)
            {
                CommonUtility.SetObjectActive(data.getSkinButton.gameObject, enabled);
                CommonUtility.SetObjectActive(data.skipSkinButton.gameObject, enabled);
            }

        }

        private void SkipSkinButton_OnClick()
        {
            GameServices.Instance.ProposalService.SkinProposal.SkipSkin();

            LevelProgressObserver.TriggerSkipSkin();

            resultScreen.LoadModeHideAction();
        }

        private void LoadModeHideAction()
        {
            EndHandleHideModeActionEvents();

            resultScreen.LoadModeHideAction();
        }

        private void BeginHandleHideModeActionEvents()
        {
            Scheduler.Instance.CallMethodWithDelay(this, LoadModeHideAction, data.delayAfterClaim);

            TouchManager.OnBeganTouchAnywhere += LoadModeHideAction;
        }

        private void EndHandleHideModeActionEvents()
        {
            Scheduler.Instance.UnscheduleMethod(this, LoadModeHideAction);

            TouchManager.OnBeganTouchAnywhere -= LoadModeHideAction;
        }

        #endregion
    }
}
