using System;
using System.Collections.Generic;
using Drawmasters.Helpers;
using Drawmasters.Proposal;
using Drawmasters.ServiceUtil;
using Drawmasters.Effects;
using Modules.General;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Drawmasters.Statistics.Data;
using AnimationEventsListener = Drawmasters.Helpers.AnimationEventsListener;


namespace Drawmasters.Ui
{
    [Serializable]
    public class UiLeagueBasePropose : UiLiveOpsPropose
    {
        #region Fields

        private const string PlayRootBounceId = "PlayRootBounceId";
        private const string ShowIntermediateRewardScreen = "ShowIntermediateRewardScreen";
        private const string Flare = "Flare";

        [SerializeField] private Image currentLeagueImage = default;
        [SerializeField] private Image nextLeagueImage = default;

        [SerializeField] private TMP_Text leagueLevelText = default;
        [SerializeField] private Animator leagueLevelUpAnimator = default;

        [SerializeField] private Transform proposeAnnouncerRoot = default;
        [SerializeField] private Transform proposeAnnouncerTarget = default;
        [SerializeField] private UiProposeAnnouncer proposeAnnouncer = default;

        [Header("Intermediate Reward")]
        [SerializeField] private UiLeagueIntermediateRewardProgressBar intermediateRewardProgressBar = default;
        [SerializeField] private AnimationEventsListener showIntermediateRewardScreenListener = default;
        [SerializeField] private Transform fxFlareRoot = default;

        [SerializeField] private AnimationEventsListener rootEventsListener = default;

        private GameObject announcerPlayingClickedGO;

        protected LeagueProposeController controller;

        private GameTimeScaleHelper timeScaleHelper;

        private bool isInternetAvailable;
        private bool wasIntermediateScreenShowed;

        #endregion



        #region Properties

        public override LiveOpsProposeController LiveOpsProposeController =>
            GameServices.Instance.ProposalService.LeagueProposeController;

        protected override LiveOpsEventController LiveOpsEventController =>
            GameServices.Instance.ProposalService.HappyHoursLeagueProposeController;

        protected override bool ShouldDestroyTutorialCanvasAfterClick =>
            controller.LeaderBoard.ShouldShowPreviewScreen ||
            !isInternetAvailable;

        protected override bool ShouldDestroyTutorialCanvasAfterEvent =>
            !isInternetAvailable ||
            !controller.IsActive ||
            !ShouldShowProposalRoot;

        public override bool CanForceProposeLiveOpsEvent =>
            base.CanForceProposeLiveOpsEvent &&
            isInternetAvailable;

        private bool ShouldPlaySkullsReceivedFx =>
            GameServices.Instance.ProposalService.LeagueProposeController.SkullsCountCollectOnLastLevel > 0.0f &&
            GameServices.Instance.ProposalService.LeagueProposeController.IsInternetAvailable &&
            GameServices.Instance.ProposalService.LeagueProposeController.IsActive;

        #endregion



        #region Methods

        public override void Initialize()
        {
            controller = GameServices.Instance.ProposalService.LeagueProposeController;

            isInternetAvailable = controller.IsInternetAvailable;

            // hack We should play announcer firstly, because in base method we can call live ops event start action
            proposeAnnouncer.OnShouldShowProgressBar += OnShouldProposeRewards;
            proposeAnnouncer.OnAnimationFinished += ProposeAnnouncer_OnAnimationFinished;
            controller.LeaderBoard.OnChangePlayerPosition += LeaderBoard_OnChangePlayerPosition;
        
            controller.LeaderBoard.UpdateLeaderList();

            intermediateRewardProgressBar.Initialize();
            RefreshIntermediateReward(0);
        
            PlayerCurrencyData playerCurrencyData = GameServices.Instance.PlayerStatisticService.CurrencyData;
            float lastReceivedSkullsCount = playerCurrencyData.GetEarnedCurrency(CurrencyType.Skulls) - controller.SkullsCountCollectOnLastLevel;
            int currentPlayerPositionIndex = ShouldPlaySkullsReceivedFx ?
                controller.PreviousPlayerPosition : controller.LeaderBoard.CurrentPlayerPosition;

            SetPlayerPosition(currentPlayerPositionIndex + 1);

            if (ShouldPlaySkullsReceivedFx)
            {
                wasIntermediateScreenShowed = false;

                ShouldDeinitializeButtons();

                timeScaleHelper = new GameTimeScaleHelper(controller.VisualSettings.timeScaleForProposalAnnouncer);
                timeScaleHelper.Initialize();

                PlayCollectSkullFx(controller.IntermediateRewardController.ReceivedLeaguePoints);

                TouchManager.OnBeganTouchAnywhere += TouchManager_OnBeganTouchAnywhere;
            }

            base.Initialize();
        }


        public override void Deinitialize()
        {
            proposeAnnouncer.OnShouldShowProgressBar -= OnShouldProposeRewards;
            controller.LeaderBoard.OnChangePlayerPosition -= LeaderBoard_OnChangePlayerPosition;
            proposeAnnouncer.OnAnimationFinished -= ProposeAnnouncer_OnAnimationFinished;

            Scheduler.Instance.UnscheduleAllMethodForTarget(this);

            timeScaleHelper?.Deinitialize();
            intermediateRewardProgressBar.Deinitialize();

            showIntermediateRewardScreenListener.Clear();
            rootEventsListener.Clear();

            base.Deinitialize();
        }


        protected override void OnPreForcePropose()
        {
            base.OnPreForcePropose();

            // To fix weird bug with text's color
            CommonUtility.SetObjectActive(leagueLevelText.gameObject, false);
            CommonUtility.SetObjectActive(leagueLevelText.gameObject, true);
        }


        private void PlayCollectSkullFx(float receivedSkullsCount)
        {
            controller.LeaderBoard.OnChangePlayerPosition -= LeaderBoard_OnChangePlayerPosition;

            proposeAnnouncer.SetupFxKey(EffectKeys.FxGUIMenuSkullsFly);
            proposeAnnouncer.Show(proposeAnnouncerRoot,
                                  proposeAnnouncerTarget,
                                  string.Concat("+", receivedSkullsCount.ToShortFormat()));
        }


        private void SetPlayerPosition(int position) =>
            leagueLevelText.text = position.ToString();


        private void RefreshIntermediateReward(float pointesCollected, Action fallbackCallback = default)
        {
            bool isActive = controller.IntermediateRewardController.IsActive;

            intermediateRewardProgressBar.SetObjectActive(controller.IntermediateRewardController.IsActive);
            int earnedRewardsPerStage = controller.IntermediateRewardController.EarnedRewardsPerStage;
            intermediateRewardProgressBar.SetupReceivedRewards(earnedRewardsPerStage);

            if (!isActive)
            {
                fallbackCallback?.Invoke();
                return;
            }

            SetBounds();
            SetChestsVisual();
            UpdateProgress();


            void SetBounds()
            {
                int bottomBound = controller.IntermediateRewardController.IntermediateRewardPointsBottomBound;
                int topBound = controller.IntermediateRewardController.IntermediateRewardPointsTopBound;

                intermediateRewardProgressBar.SetBounds(bottomBound, topBound);
            }


            void SetChestsVisual()
            {
                ChestType[] chestTypesOnCurrentStage = controller.IntermediateRewardController.ChestsOnCurrentStage;

                List<Sprite> sprites = new List<Sprite>(chestTypesOnCurrentStage.Length);

                foreach (var chestType in chestTypesOnCurrentStage)
                {
                    sprites.Add(controller.VisualSettings.FindIntermediateMenuChestIcon(chestType));
                }

                intermediateRewardProgressBar.SetChestsVisual(sprites.ToArray());
            }


            void UpdateProgress()
            {
                float pointsCount = controller.IntermediateRewardController.LeaguePointsCount;

                intermediateRewardProgressBar.UpdateProgress(pointsCount, pointsCount + pointesCollected);
            }
        }


        private void PlayLevelUpFx()
        {
            rootEventsListener.AddListener(Flare, () =>
            {
                EffectManager.Instance.PlaySystemOnce(EffectKeys.FxGUIMenuCupShine,
                                                      parent: fxFlareRoot,
                                                      transformMode: TransformMode.Local);
            });

            rootEventsListener.AddListener(PlayRootBounceId, () =>
            {
                controller.LeaderBoard.UpdateLeaderList();
                controller.IntermediateRewardController.ApplyLeaguePoints();
                controller.ResetSkullsCountCollectOnLastLevel();

                wasIntermediateScreenShowed = controller.RewardClaimController.CanClaimReward;

                OnCompleteSequenceElement?.Invoke();

                EffectHandler effectHandler = EffectManager.Instance.PlaySystemOnce(EffectKeys.FxGUIMapElementUnlock,
                                                                      parent: transform,
                                                                      transformMode: TransformMode.Local);

                effectHandler.transform.position = proposeAnnouncerTarget.position;
                effectHandler.transform.localScale *= 5.0f;

                int currentPlayerPosition = controller.LeaderBoard.CurrentPlayerPosition + 1;
                SetPlayerPosition(currentPlayerPosition);

                controller.LeaderBoard.OnChangePlayerPosition += LeaderBoard_OnChangePlayerPosition;
            });

            leagueLevelUpAnimator.SetTrigger(AnimationKeys.Common.Bounce);

        }


        private void ShowIntermediateRewardCallback()
        {
            TouchManager.OnBeganTouchAnywhere -= TouchManager_OnBeganTouchAnywhere;

            timeScaleHelper.Deinitialize();
            timeScaleHelper = null;

            ShouldInitializeButtons();
            InvokeShouldSetLockedProposals(false);
        }


        public override void StartSequenceElementExecution(ProposeSequence sequence)
        {      
            if (ShouldPlaySkullsReceivedFx)
            {
                sequence.OnComplete.AddListener(() =>
                {
                    if (!controller.NeedAnyPropose)
                    {
                        if (!wasIntermediateScreenShowed)
                        {
                            // hotfix If there hadn't been delay, then touch simulation would have been completed without buttons onClick callback. To DMITRY S
                            const float simulateTouchDelay = 0.3f;
                            Scheduler.Instance.CallMethodWithDelay(this, () =>
                                EventSystemController.SimulateTouch(announcerPlayingClickedGO), simulateTouchDelay);
                        }

                        AttemptForcePropose();
                    }
                });

                return;
            }

            base.StartSequenceElementExecution(sequence);
        }


        #endregion



        #region Events handlers

        protected override void OnClickOpenButton(bool isForcePropose)
        {
            if (!controller.IsInternetAvailable)
            {
                UiScreenManager.Instance.ShowScreen(ScreenType.NoInternet);
                return;
            }

            if (controller.LeaderBoard.ShouldShowPreviewScreen)
            {
                UiScreenManager.Instance.ShowScreen(ScreenType.LeaguePreview,
                    onShowed: (view) => SetFadeEnabled(false));
                controller.LeaderBoard.ShouldShowPreviewScreen = false;
            }
            else
            {
                DeinitializeButtons();
                controller.Propose();
            }
        }


        protected override void OnRefreshTimeLeft()
        {
            base.OnRefreshTimeLeft();

            if (isInternetAvailable != controller.IsInternetAvailable)
            {
                isInternetAvailable = controller.IsInternetAvailable;
                OnShouldRefreshVisual();
                InvokeForceProposeLiveOpsEvent();
            }
        }


        protected override void OnShouldRefreshVisual()
        {
            base.OnShouldRefreshVisual();

            if (controller.IsInternetAvailable)
            {
                LeagueType currentLeagueType = controller.WasFirstLiveOpsStarted ? controller.LeaderBoard.LeagueType : LeagueType.Bronze;

                currentLeagueImage.sprite = controller.VisualSettings.FindEnabledLeagueIconSprite(currentLeagueType);
                nextLeagueImage.sprite = controller.VisualSettings.FindEnabledLeagueIconSprite(currentLeagueType);
            }
            else
            {
                currentLeagueImage.sprite = controller.VisualSettings.FindOfflineLeagueIconSprite();
                nextLeagueImage.sprite = controller.VisualSettings.FindOfflineLeagueIconSprite();
            }

            CommonUtility.SetObjectActive(leagueLevelText.gameObject, controller.IsInternetAvailable);
        }


        private void OnShouldProposeRewards()
        {
            showIntermediateRewardScreenListener.AddListener(ShowIntermediateRewardScreen, ShowIntermediateRewardCallback);

            RefreshIntermediateReward(controller.IntermediateRewardController.ReceivedLeaguePoints, ShowIntermediateRewardCallback);
        }


        private void ProposeAnnouncer_OnAnimationFinished() =>
            PlayLevelUpFx();

        private void LeaderBoard_OnChangePlayerPosition(int prevPosition, int newPosition) =>
            SetPlayerPosition(newPosition + 1);


        private void TouchManager_OnBeganTouchAnywhere()
        {
            TouchManager.OnBeganTouchAnywhere -= TouchManager_OnBeganTouchAnywhere;

            announcerPlayingClickedGO = EventSystemController.CurrentSelectedGameObject;

        }
        #endregion
    }
}
