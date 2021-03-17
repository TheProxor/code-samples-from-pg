using System;
using System.Collections.Generic;
using DG.Tweening;
using Modules.General.Abstraction;
using Drawmasters.Advertising;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Drawmasters.ServiceUtil.Interfaces;
using Drawmasters.ServiceUtil;
using Drawmasters.Levels.Data;
using Drawmasters.Levels;
using Drawmasters.Proposal;
using Drawmasters.Proposal.Ui;
using Drawmasters.Utils;
using Modules.Advertising;
using Modules.Analytics;
using Modules.General;


namespace Drawmasters.Ui
{
    public class ResultCommonCompleteBehaviour : IResultBehaviour
    {
        #region Nested types

        [Serializable]
        public class CurrencyData
        {
            public CurrencyType currencyType = default;
            public GameObject showRoot = default;
            public TMP_Text currencyCounter = default;

            public Graphic[] graphics = default;
        }

        #endregion


        #region Fields

        [Serializable]
        public class Data
        {
            [Required] public GameObject rootObject = default;
            [Required] public GameObject headerRootObject = default;
            [Required] public TMP_Text headerIndexText = default;
            
            [Required] public GameObject skinBarRootObject = default;
            [Required] public TMP_Text skinProgressText = default;
            [Required] public Image skinProgressFill = default;
            
            [Required] public GameObject currencyRoot = default;
            [Required] public Transform[] currencyLines = default;
            [Required] public CurrencyData[] currencyData = default;

            [Required] public RewardedVideoButton multiplyRewardButton = default;
            [Required] public TMP_Text multiplyRewardText = default;
            [Required] public GameObject gemMultiplyRewardButtonIcon = default;
            [Required] public GameObject coinMultiplyRewardButtonIcon = default;

            [Header("Usual reward button")]
            [Required] public Button usualRewardButton = default;
            [Required] public Animator usualRewardButtonAnimator = default;

            [Required] public TMP_Text usualRewardText = default;
            [Required] public GameObject gemUsualRewardIcon = default;
            [Required] public GameObject coinUsualRewardIcon = default;

            [Required] public GameObject usualNextButtonRoot = default;
            [Required] public GameObject currencyNextButtonRoot = default;

            [Header("Happy hours.")]
            public UiLiveOpsEvent uiHappyHoursLeague = default;
            public UiLiveOpsEvent uiHappyHoursSeasonEvent = default;

            [Header("Pet")]
            public ResultPetCompleteBehaviour.Data petBehaviourData = default;
        }

        protected readonly Data data;

        private readonly List<GameObject> objects;
        private readonly List<UiResultHappyHoursCurrencyHelper> currencyHelpers;

        protected readonly ILevelEnvironment levelEnvironment;
        protected readonly IProposalService proposalService;
        private readonly ICommonStatisticsService commonStatistic;

        private readonly ResultScreen resultScreen;

        private LoopedInvokeTimer timeLeftRefreshTimer;

        protected int passedSecondsAfterShowScreen;
        
        private readonly ResultPetCompleteBehaviour petBehaviour;

        #endregion



        #region Properties

        protected virtual CurrencyType BonusCurrencyType =>
            CurrencyType.Simple;


        protected virtual bool WasEarnedBonusCurrency =>
            levelEnvironment.Progress.TotalCurrencyPerLevelEndWithoutBonus(BonusCurrencyType) > 0.0f;


        protected virtual bool IsProposalSkinAvailable =>
            GameServices.Instance.ProposalService.SkinProposal.CanPropose;


        private bool IsCurrencyBonusAvailable
        {
            get
            {
                bool result = true;
                
                result &= GameServices.Instance.ProposalService.IngameCurrencyMultiplier.IsAvailable;
                result &= !levelEnvironment.Progress.WasCurrencyBonusClaimed;
                result &= WasEarnedBonusCurrency;

                return result;
            }
        }

        #endregion



        #region Ctor

        public ResultCommonCompleteBehaviour(Data _data, ResultScreen screen)
        {
            data = _data;

            resultScreen = screen;

            currencyHelpers = new List<UiResultHappyHoursCurrencyHelper>();
            objects = new List<GameObject>() { data.rootObject,
                                               data.headerRootObject };

            levelEnvironment = GameServices.Instance.LevelEnvironment;
            commonStatistic = GameServices.Instance.CommonStatisticService;
            proposalService = GameServices.Instance.ProposalService;

            petBehaviour = new ResultPetCompleteBehaviour(_data.petBehaviourData, levelEnvironment);
        }

        #endregion



        #region IDeinitializable

        public void Deinitialize()
        {
            timeLeftRefreshTimer?.Stop();

            foreach (var helper in currencyHelpers)
            {
                helper.Deinitialize();
            }

            currencyHelpers.Clear();

            data.uiHappyHoursLeague.Deinitialize();
            data.uiHappyHoursSeasonEvent.Deinitialize();

            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
            DOTween.Kill(this, true);

            objects.ForEach(go => CommonUtility.SetObjectActive(go, false));

            petBehaviour.Deinitialize();
        }

        #endregion



        #region IResultBehaviour

        public void Enable()
        {
            objects.ForEach(go => CommonUtility.SetObjectActive(go, true));

            passedSecondsAfterShowScreen = 0;
                
            LevelContext levelContext = levelEnvironment.Context;

            if (IsProposalSkinAvailable)
            {
                float skinProgress = proposalService.SkinProposal.CurrentSkinProgress;
                float skinDeltaProgress = proposalService.SkinProposal.SkinProgressDelta;

#warning need refactoring
                // cuz we set skin progress before call proposal scene
                bool isSceneMode = levelContext.SceneMode.IsSceneMode();

                float skinBeginProgress = isSceneMode ? (skinProgress - skinDeltaProgress) : skinProgress;
                float skinEndProgress = isSceneMode ? skinProgress : (skinProgress + skinDeltaProgress);

                skinBeginProgress = Mathf.Max(0.0f, skinBeginProgress);
                skinEndProgress = Mathf.Min(skinEndProgress, 1.0f);

                if (skinEndProgress >= 1.0f)
                {
                    LevelProgressObserver.TriggerBarFillingShown();
                }

                RunBarFill(skinBeginProgress, skinEndProgress);
            }

            CommonUtility.SetObjectActive(data.skinBarRootObject, IsProposalSkinAvailable);

            RefreshCurrencyInfo();

            GameMode mode = levelContext.Mode;

            int index = commonStatistic.GetLevelsFinishedCount(mode);

            if (!mode.IsHitmastersLiveOps())
            {
                // hack hotfix for result from proposal scene.
                // Cuz before show proposal we increment finished level count
                index = levelContext.SceneMode.IsProposalSceneMode() ? index - 1 : index;
            }

            string indexText = (index + 1).ToString();
            data.headerIndexText.text = indexText;

            bool isBonusAvailable = IsCurrencyBonusAvailable;

            CommonUtility.SetObjectActive(data.multiplyRewardButton.gameObject, isBonusAvailable);
            data.multiplyRewardButton.Initialize(AdsVideoPlaceKeys.CurrencyBonus);
            data.multiplyRewardButton.DeinitializeButtons();

            data.multiplyRewardButton.InitializeButtons();
            data.multiplyRewardButton.OnVideoShowEnded += BonusButton_OnVideoShowEnded;
            data.multiplyRewardButton.OnClick += BonusButton_OnClick;

            data.usualRewardButton.onClick.AddListener(UsualRewardButton_OnClick);

            RefreshCurrencyIcons();

            HappyHoursLeagueProposeController happyHoursLeagueController = proposalService.HappyHoursLeagueProposeController;
            InitializeLiveOpsEvent(happyHoursLeagueController, happyHoursLeagueController.VisualSettings, ref data.uiHappyHoursLeague, CurrencyType.Skulls);

            HappyHoursSeasonEventProposeController happyHoursControllerSeasonEvent = proposalService.HappyHoursSeasonEventProposeController;
            InitializeLiveOpsEvent(happyHoursControllerSeasonEvent, happyHoursControllerSeasonEvent.VisualSettings, ref data.uiHappyHoursSeasonEvent, CurrencyType.SeasonEventPoints);

            petBehaviour.Initialize();
            petBehaviour.PlayAnimation();

            void InitializeLiveOpsEvent(LiveOpsEventController liveOpsEventController, HappyHoursVisualSettings visualSettings, ref UiLiveOpsEvent uiLiveOpsEvent, CurrencyType currencyType)
            {
                uiLiveOpsEvent.Initialize(liveOpsEventController);
                uiLiveOpsEvent.SetForceProposePlaceAllowed(false);

                UiResultHappyHoursCurrencyHelper currencyHelper = new UiResultHappyHoursCurrencyHelper(data.currencyData, AudioKeys.Ui.SKULL_ADD);
                currencyHelper.PlayAnimation(liveOpsEventController, visualSettings, currencyType);
                currencyHelpers.Add(currencyHelper);
            }

            CommonUtility.SetObjectActive(data.usualRewardButton.gameObject, false);

            float standardRewardTimer = GameServices.Instance.AbTestService.CommonData.standardRewardTimer;
            Scheduler.Instance.CallMethodWithDelay(this, () =>
            {
                CommonUtility.SetObjectActive(data.usualRewardButton.gameObject, true);
                data.usualRewardButtonAnimator.SetTrigger(AnimationKeys.Screen.Show);
                data.usualRewardButtonAnimator.Update(default);
            }, standardRewardTimer);

            timeLeftRefreshTimer = timeLeftRefreshTimer ?? new LoopedInvokeTimer(RefreshTimeLeft);
            timeLeftRefreshTimer.Start();
        }


        public void Disable()
        {
            timeLeftRefreshTimer?.Stop();

            foreach (var helper in currencyHelpers)
            {
                helper.Deinitialize();
            }

            currencyHelpers.Clear();

            data.uiHappyHoursLeague.Deinitialize();
            data.uiHappyHoursSeasonEvent.Deinitialize();

            petBehaviour.Deinitialize();

            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
            DOTween.Kill(this, true);

            objects.ForEach(go => CommonUtility.SetObjectActive(go, false));

            DeinitializeButtons();
        }

        public void InitializeButtons() { }

        public void DeinitializeButtons()
        {
            data.multiplyRewardButton.OnVideoShowEnded -= BonusButton_OnVideoShowEnded;
            data.multiplyRewardButton.OnClick -= BonusButton_OnClick;
            data.multiplyRewardButton.Deinitialize();

            data.usualRewardButton.onClick.RemoveListener(UsualRewardButton_OnClick);
        }

        #endregion



        #region Protected methods

        protected virtual string MultipliedRewardLabel(CurrencyType type) =>
            GameServices.Instance.LevelEnvironment.Progress.UiTotalCurrencyPerLevelEndWithBonus(BonusCurrencyType, passedSecondsAfterShowScreen);


        protected virtual string UsualRewardLabel(CurrencyType type) =>
            GameServices.Instance.LevelEnvironment.Progress.UiTotalCurrencyPerLevelEndWithoutBonus(BonusCurrencyType);

        #endregion


        #region Private methods

        private void RefreshTimeLeft()
        {
            passedSecondsAfterShowScreen++;
            RefreshCurrencyIcons();
            RefreshCurrencyInfo();
        }
        
        
        private void RunBarFill(float from, float to)
        {
            ChangeBarFill(from);

            DOTween.To(() => from, ChangeBarFill, to, 1.5f)
                .SetId(this)
                .SetEase(Ease.Linear);
                     
            void ChangeBarFill(float value)
            {
                data.skinProgressFill.fillAmount = value;
                data.skinProgressText.text = value.ToPercents() + "%";
            }
        }


        private void RefreshCurrencyIcons()
        {
            data.gemMultiplyRewardButtonIcon.gameObject.SetActive(BonusCurrencyType == CurrencyType.Premium);
            data.coinMultiplyRewardButtonIcon.gameObject.SetActive(BonusCurrencyType == CurrencyType.Simple);
            
            data.gemUsualRewardIcon.gameObject.SetActive(BonusCurrencyType == CurrencyType.Premium);
            data.coinUsualRewardIcon.gameObject.SetActive(BonusCurrencyType == CurrencyType.Simple);

            string newText = MultipliedRewardLabel(BonusCurrencyType);

            data.multiplyRewardText.text = newText;
            data.usualRewardText.text = UsualRewardLabel(BonusCurrencyType);
            
            CommonUtility.SetObjectActive(data.usualNextButtonRoot, !WasEarnedBonusCurrency);
            CommonUtility.SetObjectActive(data.currencyNextButtonRoot, WasEarnedBonusCurrency);
        }


        protected virtual void RefreshCurrencyInfo()
        {
            data.currencyRoot.SetActive(true);
            
            LevelProgress progress = levelEnvironment.Progress;

            int indexColumn = 0;
            int indexRow = 0;
            List<HorizontalLayoutGroup> layoutGroups = new List<HorizontalLayoutGroup>();
            Array.ForEach(data.currencyLines, e => CommonUtility.SetObjectActive(e.gameObject, false));

            bool shouldRowShow = false;
            
            foreach (var item in data.currencyData)
            {
                if (indexRow >= data.currencyLines.Length)
                {
                    break;
                }

                bool shouldShow = progress.ShouldShowCurrencyOnResult(item.currencyType) &&
                                  item.currencyType != BonusCurrencyType;

                if (item.currencyType == CurrencyType.Skulls)
                {
                    shouldShow &= GameServices.Instance.ProposalService.LeagueProposeController.IsActive &&
                                  GameServices.Instance.ProposalService.LeagueProposeController.IsInternetAvailable;    
                }

                shouldRowShow |= shouldShow;
                
                CommonUtility.SetObjectActive(data.currencyLines[indexRow].gameObject, shouldRowShow);

                if (!shouldShow)
                {
                    continue;
                }

                CommonUtility.SetObjectActive(item.showRoot, shouldShow);
                item.showRoot.transform.SetParent(data.currencyLines[indexRow]);

                HorizontalLayoutGroup layout = item.showRoot.GetComponent<HorizontalLayoutGroup>();

                if (layout != null)
                {
                    layout.childAlignment = TextAnchor.MiddleCenter;
                    layoutGroups.Add(layout);
                }
                
                item.currencyCounter.text = progress.UiTotalCurrencyPerLevelEnd(item.currencyType);


                if (indexColumn > 0)
                {
                    indexRow++;
                    indexColumn = 0;
                    shouldRowShow = false;
                    layoutGroups.Clear();
                }
                else
                {
                    indexColumn++;
                }
            }

            if (layoutGroups.Count == 1)
            {
                layoutGroups.First().childAlignment = TextAnchor.UpperCenter;
            }
        }


        private void HideResult()
        {
            bool shouldShowSkinPropose = GameServices.Instance.LevelEnvironment.Progress.WasBarFillingShown;
            bool canClaim = GameServices.Instance.ProposalService.SkinProposal.CanClaimSkin;

            if (shouldShowSkinPropose && canClaim)
            {
                resultScreen.SwitchState(AnimationKeys.ResultScreen.CommonToSkinClaim);

                //TODO possibe multiple deinitialize
                // DisableButtons();
            }
            else
            {
                AdvertisingManager.Instance.TryShowAdByModule(AdModule.Interstitial,
                    AdPlacementType.AfterResult,
                    status => resultScreen.LoadModeHideAction());
            }
        }

        #endregion



        #region Events handlers

        private void BonusButton_OnVideoShowEnded(AdActionResultType result)
        {
            if (result == AdActionResultType.Success)
            {
                CommonEvents.SendAdVideoReward(data.multiplyRewardButton.Placement);

                LevelProgressObserver.TriggerClaimCurrencyBonus(BonusCurrencyType);

                HideResult();
            }
        }


        private void BonusButton_OnClick()
        {
            timeLeftRefreshTimer?.Stop();
        }


        private void UsualRewardButton_OnClick() =>
            HideResult();

        #endregion
    }
}