using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Modules.Sound;
using UnityEngine;
using UnityEngine.UI;
using Drawmasters.Levels;
using Drawmasters.Effects;
using Drawmasters.Helpers;
using Drawmasters.Proposal;
using Drawmasters.Tutorial;
using Drawmasters.Vibration;
using Drawmasters.ServiceUtil;
using Drawmasters.Advertising;
using Drawmasters.Analytic;
using Drawmasters.Interfaces;
using Drawmasters.Proposal.Interfaces;
using Modules.General;
using Modules.General.Abstraction;


namespace Drawmasters.Ui
{
    public class UiMonopolyScreen : RewardReceiveScreen
    {
        #region Nested types

        [Serializable]
        private class BestRewardVisual
        {
            public RewardType rewardType = default;
            public GameObject root = default;
        }

        #endregion



        #region Fields

        public static event Action OnShouldFinishLiveOps;
        public static event Action OnRollsStarted;
        public static event Action OnRollsFinished;

        [SerializeField] private Button closeButton = default;
        [SerializeField] private Image currentRollAnnouncer = default;

        [SerializeField] private TMP_Text timeLeftText = default;

        [SerializeField] private MonopolyDeskElement[] elements = default;
        [SerializeField] private MonopolyLapsElement[] lapsRewards = default;
        [SerializeField] private MonopolyCharacter character = default;
        [SerializeField] private MonopolyLapsBar monopolyLapsBar = default;

        [Header("Buttons")]
        [Header("Player roll button settings")]
        [SerializeField] private Button freeRollButton = default;
        [SerializeField] private TMP_Text currentRollBonesCountText = default;

        [Header("Currency button settings")]
        [SerializeField] private Button currencyRollButton = default;
        [SerializeField] private TMP_Text currencyForRollText = default;
        [SerializeField] private TMP_Text currencyRollButtonBonesCount = default;

        [Header("Ads button settings")]
        [SerializeField] private RewardedVideoButton adsRollButton = default;

        [SerializeField] private GameObject adsRollButtonRoot = default;
        [SerializeField] private GameObject adsRollAvailableRoot = default;
        [SerializeField] private GameObject adsRollCooldownRoot = default;
        [SerializeField] private TMP_Text timerText = default;

        [SerializeField] private TMP_Text adsRollButtonBonesCount = default;

        [Header("Fxs")]
        [SerializeField] private IdleEffect idleShineEffect = default;
        [SerializeField] private IdleEffect idleShineFrameEffect = default;

        [Header("Rotate animation")]
        [SerializeField] private SettingsScreenButton autoRollButton = default;
        [SerializeField] private GameObject autoRollButtonRoot = default;

        [Header("Dice")]
        [SerializeField] private GameObject diceRoot = default;
        [SerializeField] private Animator diceAnimator = default;
        [SerializeField] private Transform diceNumbersTransform = default;

        private MonopolyCurrencyAnnouncers monopolyCurrencyAnnouncers;

        private RewardData[] deskReward;
        private RewardData[] mainReward;
        private MonopolyProposeController controller;

        private MonopolyDeskElement lastWinDeskElement;

        private object announcerShowId;

        private Action hide;

        private Coroutine rollButtonTutorialRoutine;
        private bool isMovementActive;

        private RewardDataMergeHelper mergeCurrencyDataHelper;

        #endregion



        #region Properties

        public override ScreenType ScreenType =>
            ScreenType.Monopoly;


        public override string RewardScreenIdleFxKey =>
            EffectKeys.FxGUIMonopolyRewardOpenShine;


        protected override Dictionary<RewardType, IRewardApplyHelper> RewardApplyHelpers =>
            helpers ?? (helpers = new Dictionary<RewardType, IRewardApplyHelper>()
            {
                {RewardType.PetSkin, new CommonSkinRewardApplyHelper(this)},
                {RewardType.ShooterSkin, new CommonSkinRewardApplyHelper(this)},
                {RewardType.WeaponSkin, new CommonSkinRewardApplyHelper(this)},
                {RewardType.Currency, new MonopolyCurrencyRewardApplyHelper(this, reward => Array.Exists(lapsRewards, e => e.RewardData == reward))},
                {
                    RewardType.SpinRouletteCash,
                    new SpinRouletteRewardApplyHelper<SpinRouletteCashReward>(IngameData.Settings.seasonEvent
                        .spinRouletteCashSeasonEventRewardPackSettings)
                },
                {
                    RewardType.SpinRouletteSkin,
                    new SpinRouletteRewardApplyHelper<SpinRouletteSkinReward>(IngameData.Settings.seasonEvent
                        .spinRouletteSkinSeasonEventRewardPackSettings)
                },
                {
                    RewardType.SpinRouletteWaipon,
                    new SpinRouletteRewardApplyHelper<SpinRouletteWaiponReward>(IngameData.Settings.seasonEvent
                        .spinRouletteWaiponSeasonEventRewardPackSettings)
                },
                {
                    RewardType.Forcemeter,
                    new ForcemeterRewardApplyHelper(IngameData.Settings.seasonEvent
                        .forceMeterSeasonEventRewardPackSettings)
                },
                {RewardType.None, new DefaultRewardApplyHelper()}
            });

        #endregion



        #region Methods

        public void SetDeskReward(RewardData[] _deskReward)
        {
            deskReward = _deskReward;

            for (int i = 0; i < elements.Length && i < deskReward.Length; i++)
            {
                elements[i].SetupReward(deskReward[i]);
            }

            int currentDeskIndex = controller.MonopolyLiveOpsDeskCounter == 0 ?
                0 : controller.MonopolyLiveOpsDeskCounter % deskReward.Length;
            lastWinDeskElement = currentDeskIndex < elements.Length ? elements[currentDeskIndex] : default;

            Vector3 characterEndPosition = ((RectTransform)lastWinDeskElement.transform).anchoredPosition3D;
            character.MoveToPosition(characterEndPosition, MonopolyCharacter.JumpDirection.None, null, true);

            for (int i = 0; i < elements.Length && i < deskReward.Length; i++)
            {
                bool isClaimed = i <= currentDeskIndex;
                elements[i].SetClaimed(isClaimed, true);

                if (i == currentDeskIndex)
                {
                    elements[i].PlayBumpAnimation(true, true);
                }
            }
        }


        public void SetMainReward(RewardData[] _mainReward)
        {
            mainReward = _mainReward;

            for (int i = 0; i < lapsRewards.Length && i < mainReward.Length; i++)
            {
                lapsRewards[i].SetupReward(mainReward[i]);
                lapsRewards[i].SetLapsesInfo(controller.Settings.CountsLapsForReward[i]);
            }
            
            bool[] isRewardCollected = controller.CollectedLapsReward(controller.MonopolyLiveOpsDeskCounter);
            for (int i = 0; i < isRewardCollected.Length && i < lapsRewards.Length; i++)
            {
                lapsRewards[i].SetClaimed(isRewardCollected[i], true);

                if (!isRewardCollected[i])
                {
                    lapsRewards[i].ShowIdleEffect();
                }
            }
        }


        public void RefreshVisual()
        {
            for (int i = 0; i < elements.Length; i++)
            {
                elements[i].RefreshVisual();
            }

            for (int i = 0; i < lapsRewards.Length; i++)
            {
                lapsRewards[i].RefreshVisual();
            }
        }

        

        public override void Initialize(Action<AnimatorView> onShowEndCallback = null,
                                        Action<AnimatorView> onHideEndCallback = null,
                                        Action<AnimatorView> onShowBeginCallback = null,
                                        Action<AnimatorView> onHideBeginCallback = null)
        {
            base.Initialize(onShowEndCallback, onHideEndCallback, onShowBeginCallback, onHideBeginCallback);

            controller = GameServices.Instance.ProposalService.MonopolyProposeController;

            announcerShowId = Guid.NewGuid();

            uiHudTop.InitializeCurrencyRefresh();
            uiHudTop.SetupExcludedTypes(CurrencyType.RollBones);
            uiHudTop.RefreshCurrencyVisual(0.0f);
            monopolyCurrencyAnnouncers = new MonopolyCurrencyAnnouncers();
            monopolyCurrencyAnnouncers.Initialize();

            MonoBehaviourLifecycle.OnUpdate += MonoBehaviourLifecycle_OnUpdateRefreshTimer;
            MonoBehaviourLifecycle.OnUpdate += MonoBehaviourLifecycle_OnUpdateRefreshAdsTimer;

            controller.OnAdsProposeRefresh += RefreshAdsButton;

            idleShineEffect.CreateAndPlayEffect();
            idleShineFrameEffect.CreateAndPlayEffect();

            currencyForRollText.text = controller.Settings.rewardForCurrencyBuy.price.ToShortFormat();

            adsRollButton.Initialize(AdsVideoPlaceKeys.MonopolyScreenRollBones);
            adsRollButton.OnVideoShowEnded += AdsRollButton_OnVideoShowEnded;

            RefreshCurrentRollBonesCount();
            adsRollButtonBonesCount.text = string.Concat("+", controller.Settings.rewardForAds.UiRewardText);
            currencyRollButtonBonesCount.text = string.Concat("+", controller.Settings.rewardForCurrencyBuy.UiRewardText);

            RefreshButtonsUi();

            autoRollButton.Initialize();
            autoRollButton.SetEnabled(controller.IsAutoRollEnabled);

            character.Initialize();
            monopolyLapsBar.Initialize();

            SetCurrentLaps();

            CommonUtility.SetObjectActive(diceRoot, false);

            currentRollAnnouncer.color = currentRollAnnouncer.color.SetA(0.0f);
            currentRollAnnouncer.transform.localScale = Vector3.zero;

            RefreshAdsButton();
            MonoBehaviourLifecycle_OnUpdateRefreshAdsTimer(default);

            mergeCurrencyDataHelper = new RewardDataMergeHelper();

            TouchManager.OnBeganTouchAnywhere += TouchManager_OnBeganTouchAnywhere;
        }


        public override void Deinitialize()
        {
            TutorialFingerScreen screen = UiScreenManager.Instance.LoadedScreen<TutorialFingerScreen>(ScreenType.TutorialFinger);

            if (screen != null)
            {
                screen.OnHideEnd -= Screen_OnHideEnd;
                screen.HideImmediately();
            }

            MonoBehaviourLifecycle.StopPlayingCorotine(rollButtonTutorialRoutine);

            uiHudTop.DeinitializeCurrencyRefresh();
            monopolyCurrencyAnnouncers.Deinitialize();

            TouchManager.OnBeganTouchAnywhere -= TouchManager_OnBeganTouchAnywhere;
            MonoBehaviourLifecycle.OnUpdate -= MonoBehaviourLifecycle_OnUpdateRefreshTimer;
            MonoBehaviourLifecycle.OnUpdate -= MonoBehaviourLifecycle_OnUpdateRefreshAdsTimer;

            controller.OnAdsProposeRefresh -= RefreshAdsButton;

            DOTween.Kill(announcerShowId);

            Scheduler.Instance.UnscheduleAllMethodForTarget(this);

            deskReward = null;
            foreach (var element in elements)
            {
                element.Deinitialize();
            }

            foreach (var element in lapsRewards)
            {
                element.StopIdleEffect();
                element.Deinitialize();
            }

            idleShineEffect.StopEffect();
            idleShineFrameEffect.StopEffect();

            hide?.Invoke();

            adsRollButton.Deinitialize();
            adsRollButton.DeinitializeButtons();

            adsRollButton.OnVideoShowEnded -= AdsRollButton_OnVideoShowEnded;

            autoRollButton.Deinitialize();

            character.Deinitialize();
            monopolyLapsBar.Deinitialize();

            mergeCurrencyDataHelper.Deinitialize();

            base.Deinitialize();
        }


        public override void Show()
        {
            base.Show();

            RefreshButtonsUi();
            RefreshInactivityRollButtonTutorial();

            SoundManager.Instance.PlayOneShot(AudioKeys.Ui.SHOPENTER);
        }


        public override void InitializeButtons()
        {
            adsRollButton.InitializeButtons();

            closeButton.onClick.AddListener(CloseButton_OnClick);
            freeRollButton.onClick.AddListener(RollButton_OnClick);
            currencyRollButton.onClick.AddListener(CurrencyRollButton_OnClick);

            InitializeAutoRollButtons();
        }


        public override void DeinitializeButtons()
        {
            adsRollButton.DeinitializeButtons();

            closeButton.onClick.RemoveListener(CloseButton_OnClick);

            freeRollButton.onClick.RemoveListener(RollButton_OnClick);
            currencyRollButton.onClick.RemoveListener(CurrencyRollButton_OnClick);

            DeinitializeAutoRollButtons();
        }


        private void InitializeAutoRollButtons()
        {
            autoRollButton.AddButtonOnClickCallback(EnableAutoRoll, false);
            autoRollButton.AddButtonOnClickCallback(DisableAutoRoll, true);
        }


        private void DeinitializeAutoRollButtons()
        {
            autoRollButton.RemoveButtonOnClickCallback(EnableAutoRoll, false);
            autoRollButton.RemoveButtonOnClickCallback(DisableAutoRoll, true);
        }


        public void AddOnHideCallback(Action callback) =>
            hide += callback;
        

        // also used in Unity Animation Event
        private void RefreshButtonsUi()
        {
            CommonUtility.SetObjectActive(freeRollButton.gameObject, controller.IsPlayerRollAvailable);
            CommonUtility.SetObjectActive(autoRollButtonRoot, controller.IsPlayerRollAvailable);

            CommonUtility.SetObjectActive(currencyRollButton.gameObject, !controller.IsPlayerRollAvailable);
            CommonUtility.SetObjectActive(adsRollButtonRoot, !controller.IsPlayerRollAvailable);
        }


        private void SetCurrentLaps()
        {
            int currentLap = controller.MonopolyLiveOpsDeskCounter / controller.Settings.DescMovementsForLaps;
            currentLap = Mathf.Min(currentLap, controller.Settings.CountsLapsForReward.LastObject());
            monopolyLapsBar.PlayAnimation(currentLap, true);
        }


        private void PerfomDeskMoves(int movesCount, Action callback)
        {
            int movesCounter = 0;

            character.PlayStartJumpAnimation();

            PerfomIncrementDeskMove(OnStepMoved);

            void OnStepMoved()
            {
                movesCounter++;
                bool isLast = movesCounter == movesCount;
                bool isNextLapsDetected = controller.MonopolyLiveOpsDeskCounter % deskReward.Length == 0;

                if (isNextLapsDetected)
                {
                    lastWinDeskElement.ShowLapsFinishEffect();

                    SoundManager.Instance.PlayOneShot(AudioKeys.Ui.MONOPOLY_LAP_FLAG);
                    EffectManager.Instance.PlaySystemOnce(EffectKeys.FxGUIMonopolyDeckShine, 
                        transform.position, 
                        transform.rotation, 
                        transform);
                }
                else
                {
                    lastWinDeskElement.ShowWinEffect();
                }

                if (movesCounter < movesCount)
                {
                    float delay = isNextLapsDetected ? controller.VisualSettings.delayBetweenStepsNewLaps : controller.VisualSettings.delayBetweenSteps;
                    Scheduler.Instance.CallMethodWithDelay(this, () => PerfomIncrementDeskMove(OnStepMoved), delay);
                }
                else
                {
                    OnMoved();
                }
            }

            void OnMoved()
            {
                VibrationManager.Play(IngameVibrationType.RouletteEnd);

                RefreshVisual();

                character.PlayFinishJumpAnimation();

                callback?.Invoke();

                mergeCurrencyDataHelper.ApplyMergedReward(reward => OnShouldApplyReward(reward));
            }
        }

        private void PerfomIncrementDeskMove(Action callback)
        {
            controller.IncrementDeskCounter();

            bool isNextLapsDetected = controller.MonopolyLiveOpsDeskCounter % deskReward.Length == 0;

            lastWinDeskElement.PlayBumpAnimation(false, false);

            int currentDeskIndex = controller.MonopolyLiveOpsDeskCounter % deskReward.Length;
            lastWinDeskElement = currentDeskIndex < elements.Length ? elements[currentDeskIndex] : default;

            lastWinDeskElement.PlayBumpAnimation(true, false);
            Vector3 characterEndPosition = ((RectTransform)lastWinDeskElement.transform).anchoredPosition3D;

            const int cellsInRow = 5;
            int indexesInRow = cellsInRow - 1;
            bool isHorizontalMove = (currentDeskIndex > 0 && currentDeskIndex <= indexesInRow) ||
                                    (currentDeskIndex > indexesInRow * 2 && currentDeskIndex <= indexesInRow * 3);

            bool isVerticalUp = currentDeskIndex == 0 || (currentDeskIndex > indexesInRow * 3 && currentDeskIndex <= indexesInRow * 4);

            MonopolyCharacter.JumpDirection jumpDirection = MonopolyCharacter.JumpDirection.None;

            if (isHorizontalMove)
            {
                jumpDirection |= MonopolyCharacter.JumpDirection.Horizontal;
            }
            if (isVerticalUp)
            {
                jumpDirection |= MonopolyCharacter.JumpDirection.Vectical;
            }

            character.MoveToPosition(characterEndPosition, jumpDirection, () =>
             {
                 if (isNextLapsDetected)
                 {
                     foreach (var element in elements)
                     {
                         element.SetClaimed(false, false);
                     }

                     int currentLap = controller.MonopolyLiveOpsDeskCounter / controller.Settings.DescMovementsForLaps;
                     if (currentLap <= controller.Settings.CountsLapsForReward.LastObject())
                     {
                         monopolyLapsBar.PlayAnimation(currentLap, false);
                     }
                 }

                 lastWinDeskElement.SetClaimed(true, false);

                  bool[] isRewardCollected = controller.CollectedLapsReward(controller.MonopolyLiveOpsDeskCounter);

                 for (int i = 0; i < isRewardCollected.Length && i < lapsRewards.Length; i++)
                 {
                     if (!lapsRewards[i].IsClaimed && isRewardCollected[i])
                     {
                         lapsRewards[i].SetClaimed(true, false);
                         lapsRewards[i].HideIdleEffect();
                         lapsRewards[i].ShowWinEffect();

                         bool isLastReward = i == isRewardCollected.Length - 1;
                         if (isLastReward)
                         {
                             controller.IsCurrentLiveOpsTaskCompleted = true;
                             AnalyticHelper.SendLiveOpsCompleteEvent(controller.LiveOpsAnalyticName, controller.LiveOpsAnalyticEventId);
                         }
                         
                         Action onShouldApplyRewardCallback = isLastReward ? () =>
                            {
                                OnShouldFinishLiveOps?.Invoke();
                                CloseButton_OnClick();
                            }
                         : (Action)default;

                         RewardData savedRewardData = lapsRewards[i].RewardData;


                         float applyRewardDelay = isLastReward ? controller.VisualSettings.lastRewardApplyDelay : 0.0f;
                         Scheduler.Instance.CallMethodWithDelay(this, () => OnShouldApplyReward(savedRewardData, onShouldApplyRewardCallback), applyRewardDelay);

                     }
                 }

                 if (deskReward[currentDeskIndex] is CurrencyReward currencyReward)
                 {
                     monopolyCurrencyAnnouncers.PlayAnnouncer(lastWinDeskElement.transform, currencyReward);
                 }

                 RewardData currentReward = deskReward[currentDeskIndex];
                 if (!mergeCurrencyDataHelper.TryMergeReward(currentReward))
                 {
                     OnShouldApplyReward(currentReward);
                 }

                 VibrationManager.Play(IngameVibrationType.MonopolyJump);

                 callback?.Invoke();
             }, false);

            SoundManager.Instance.PlayOneShot(AudioKeys.Ui.MONOPOLY_JUMP);
        }


        private void SwitchFromFreeButton()
        {
            if (!controller.IsPlayerRollAvailable)
            {
                CommonUtility.SetObjectActive(freeRollButton.gameObject, true);
                CommonUtility.SetObjectActive(autoRollButtonRoot, true);

                CommonUtility.SetObjectActive(currencyRollButton.gameObject, true);
                CommonUtility.SetObjectActive(adsRollButtonRoot, true);

                mainAnimator.SetTrigger(AnimationKeys.MonopolyScreen.ChangeRollButtonsToFree);
            }
        }


        private void SwitchToFreeButton()
        {
            if (controller.IsPlayerRollAvailable)
            {
                CommonUtility.SetObjectActive(freeRollButton.gameObject, true);
                CommonUtility.SetObjectActive(autoRollButtonRoot, true);

                CommonUtility.SetObjectActive(currencyRollButton.gameObject, true);
                CommonUtility.SetObjectActive(adsRollButtonRoot, true);

                mainAnimator.SetTrigger(AnimationKeys.MonopolyScreen.ChangeRollButtonsFromFree);
            }
        }

        private void RefreshCurrentRollBonesCount() =>
                    currentRollBonesCountText.text = GameServices.Instance.PlayerStatisticService.CurrencyData
                                                                            .GetEarnedCurrency(CurrencyType.RollBones)
                                                                            .ToShortFormat();


        private void EnableAutoRoll()
        {
            controller.IsAutoRollEnabled = true;
            autoRollButton.SetEnabled(controller.IsAutoRollEnabled);
        }

        private void DisableAutoRoll()
        {
            controller.IsAutoRollEnabled = false;
            autoRollButton.SetEnabled(controller.IsAutoRollEnabled);
        }


        private void TouchManager_OnBeganTouchAnywhere()
        {
            bool isScreenActive = UiScreenManager.Instance.IsScreenActive(ScreenType.TutorialFinger);

            if (!isScreenActive)
            {
                RefreshInactivityRollButtonTutorial();
            }
        }


        private void RefreshInactivityRollButtonTutorial()
        {
            MonoBehaviourLifecycle.StopPlayingCorotine(rollButtonTutorialRoutine);
            rollButtonTutorialRoutine = MonoBehaviourLifecycle.PlayCoroutine(InactivityRollButtonTutorial());
        }


        private IEnumerator InactivityRollButtonTutorial()
        {
            float duration = controller.VisualSettings.tutorialInactivityDelay;
            float currentTime = default;

            while (currentTime < duration)
            {
                if (!isMovementActive && controller.IsPlayerRollAvailable)
                {
                    currentTime += Time.deltaTime;
                }

                yield return null;
            }

            TutorialManager.StartTurorial(TutorialType.Monopoly, ScreenType.TutorialFinger, RefreshInactivityRollButtonTutorial);
            TutorialFingerScreen screen = UiScreenManager.Instance.LoadedScreen<TutorialFingerScreen>(ScreenType.TutorialFinger);

            if (screen != null)
            {
                int order = mainCanvas.sortingOrder + (int)(ViewManager.OrderOffset * 0.5f);
                screen.SetSortingOrder(order);

                screen.RepositionInputCheckZone(controller.VisualSettings.tutorialTapZonePosition);
                screen.ResizeInputCheckZone(controller.VisualSettings.tutorialTapZoneSize, Vector2.one * 0.5f, Vector2.one * 0.5f);

                screen.MarkShouldCloseOnDown();

                screen.OnHideEnd += Screen_OnHideEnd;
            }
        }

        private void Screen_OnHideEnd(AnimatorView screen)
        {
            screen.OnHideEnd -= Screen_OnHideEnd;

            freeRollButton.onClick?.Invoke();
        }

        #endregion



        #region Events handlers

        private void CloseButton_OnClick()
        {
            DeinitializeButtons();

            FromLevelToLevel.PlayTransition(() =>
            {
                HideImmediately();

                LevelsManager.Instance.UnloadLevel();
                LevelsManager.Instance.LoadScene(GameServices.Instance.PlayerStatisticService.PlayerData.LastPlayedMode, GameMode.MenuScene);
                UiScreenManager.Instance.ShowScreen(ScreenType.MainMenu, isForceHideIfExist: true);
            });
        }


        private void RollButton_OnClick() =>
            PerfomPlayerRoll();


        private void PerfomPlayerRoll()
        {
            if (!controller.IsPlayerRollAvailable)
            {
                return;
            }

            IProposable proposable = new CurrencyProposal((1.0f, CurrencyType.RollBones));

            proposable.Propose((result) =>
            {
                if (result)
                {
                    isMovementActive = true;

                    DeinitializeButtons();
                    InitializeAutoRollButtons();

                    RefreshCurrentRollBonesCount();

                    int rollValue = controller.Settings.GenerateRollValue();

                    CommonUtility.SetObjectActive(diceRoot, true);
                    diceNumbersTransform.localEulerAngles = controller.VisualSettings.FindDiceNumbersRotation(rollValue);

                    Scheduler.Instance.CallMethodWithDelay(this, () =>
                    {
                        PerfomDeskMoves(rollValue, () =>
                        {
                            bool isRewardScreenActive = UiScreenManager.Instance.IsScreenActive(ScreenType.SpinReward);
                            diceAnimator.SetTrigger(AnimationKeys.MonopolyScreen.RollDiceHide);

                            if (!isRewardScreenActive && controller.IsAutoRollEnabled && controller.IsPlayerRollAvailable)
                            {
                                Scheduler.Instance.CallMethodWithDelay(this, PerfomPlayerRoll, controller.VisualSettings.autoRolldelays);
                            }
                            else
                            {
                                isMovementActive = false;

                                DeinitializeAutoRollButtons();
                                InitializeButtons();

                                SwitchFromFreeButton();

                                OnRollsFinished?.Invoke();
                            }
                        });
                    }, controller.VisualSettings.characterStartMoveDelay);

                    diceAnimator.SetTrigger(AnimationKeys.MonopolyScreen.RollDice.RandomObject());

                    DOTween.Complete(announcerShowId);

                    controller.VisualSettings.numberShowAlphaAnimation.Play(value => 
                        currentRollAnnouncer.color = currentRollAnnouncer.color.SetA(value),  
                        announcerShowId,  () => 
                        {
                            controller.VisualSettings.numberShowAlphaAnimation.Play(value => 
                                currentRollAnnouncer.color = currentRollAnnouncer.color.SetA(value), 
                                announcerShowId, null, true);
                        });

                    controller.VisualSettings.numberShowScaleAnimation.Play(value => 
                        currentRollAnnouncer.transform.localScale = value, announcerShowId, () =>
                    {
                        controller.VisualSettings.numberShowScaleAnimation.Play(value => 
                            currentRollAnnouncer.transform.localScale = value, announcerShowId, null, true);

                        SoundManager.Instance.PlayOneShot(AudioKeys.Ui.MONOPOLY_DICE_HIT_NUMBER);
                    });

                    EffectManager.Instance.PlaySystemOnce(EffectKeys.FxGUIMonopolyNumberAppear, 
                                                          currentRollAnnouncer.transform.position, 
                                                          currentRollAnnouncer.transform.rotation);

                    currentRollAnnouncer.sprite = controller.VisualSettings.FindRollAnnouncer(rollValue);
                    currentRollAnnouncer.SetNativeSize();

                    EffectManager.Instance.PlaySystemOnce(EffectKeys.FxGUIMonopolyDiceShine, 
                                                          freeRollButton.transform.position, 
                                                          freeRollButton.transform.rotation);

                    SoundManager.Instance.PlayOneShot(AudioKeys.Ui.MONOPOLY_THROW_DICE);

                    OnRollsStarted?.Invoke();
                }
            });
        }


        private void CurrencyRollButton_OnClick()
        {
            IProposable proposable = new CurrencyProposal((controller.Settings.rewardForCurrencyBuy.price, controller.Settings.rewardForCurrencyBuy.priceType));

            proposable.Propose((result) =>
            {
                if (result)
                {
                    DeinitializeButtons();

                    adsRollButton.CancelAdShowRequest();

                    OnShouldApplyReward(controller.Settings.rewardForCurrencyBuy);

                    RefreshCurrentRollBonesCount();
                    SwitchToFreeButton();

                    Scheduler.Instance.CallMethodWithDelay(this, () => InitializeButtons(), 0.5f); // hotfix for animation
                }
            });
        }


        private void AdsRollButton_OnVideoShowEnded(AdActionResultType result)
        {
            if (result == AdActionResultType.Success)
            {
                OnShouldApplyReward(controller.Settings.rewardForAds);

                RefreshCurrentRollBonesCount();
                SwitchToFreeButton();

                controller.MarkBonesAdsWatched();
            }
        }


        public override Vector3 GetCurrencyStartPosition(RewardData rewardData)
        {
            MonopolyElement foundElement = default;
            foundElement = Array.Find(elements, e => e.RewardData == rewardData);

            if (foundElement == null)
            {
                foundElement = Array.Find(lapsRewards, e => e.RewardData == rewardData);
            }

            if (foundElement == null)
            {
                CustomDebug.Log($"Can't find currency icon transform in {this}");
            }

            return foundElement == null ? default : foundElement.IconCurrencyTransform.position;
        }


        private void RefreshAdsButton()
        {
            CommonUtility.SetObjectActive(adsRollAvailableRoot, controller.IsVideoProposeAvailable);
            CommonUtility.SetObjectActive(adsRollCooldownRoot, !controller.IsVideoProposeAvailable);
        }

        #endregion



        #region Events handlers

        private void MonoBehaviourLifecycle_OnUpdateRefreshTimer(float deltaTime) =>
            timeLeftText.text = controller.TimeLeftLiveOpsUi;


        private void MonoBehaviourLifecycle_OnUpdateRefreshAdsTimer(float deltaTime) =>
            timerText.text = controller.AdsReloadUiTimeLeft;
        
        #endregion



        #region Editor methods

        [Sirenix.OdinInspector.Button]
        private void FillElements()
        {
            elements = GetComponentsInChildren<MonopolyDeskElement>();
        }

        #endregion
    }
}
