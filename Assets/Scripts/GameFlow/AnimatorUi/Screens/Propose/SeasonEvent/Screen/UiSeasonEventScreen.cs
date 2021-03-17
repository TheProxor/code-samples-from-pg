using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Spine.Unity;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Drawmasters.Levels;
using Drawmasters.Proposal;
using Drawmasters.ServiceUtil;
using Drawmasters.Effects;
using Drawmasters.Utils;
using Drawmasters.Utils.Ui;
using Drawmasters.Proposal.Ui;
using Modules.Sound;
using Modules.General;
using RewardElementState = Drawmasters.Proposal.UiSeasonEventRewardElement.State;
using Drawmasters.OffersSystem;
using I2.Loc;


namespace Drawmasters.Ui
{
    public class UiSeasonEventScreen : RewardReceiveScreen
    {
        #region Nested types

        [Serializable]
        private class PetSkinsData
        {
            public PetSkinType skinType = default;
            public GameObject[] roots = default;
        }

        #endregion



        #region Fields

        [SerializeField] private Button closeButton = default;
        [SerializeField] private Button infoButton = default;

        [SerializeField] private Localize headerText = default;
        [SerializeField] private TMP_Text timeLeftText = default;
        [SerializeField] private UiSeasonEventBar uiSeasonEventBar = default;

        [SerializeField] private PetSkinsData[] petSkinsData = default;
        [SerializeField] private GameObject[] petRewardRoot = default;
        [SerializeField] private GameObject[] commonRewardRoot = default;
        [SerializeField] private SkeletonGraphic petSkeletonGraphic = default;

        [Header("Scroll")]
        [SerializeField] private RectMask2D rectMask2D = default;
        [SerializeField] private EventTrigger scrollEventTriggers = default;
        [SerializeField] private RubberScrollRect scrollRect = default;

        [Header("Main buttons settings")]
        [SerializeField] private Button getSeasonPassButton = default;
        [SerializeField] private GameObject getSeasonPassRootCommon = default;
        [SerializeField] private GameObject getSeasonPassRootPurchased = default;

        [Header("Event reward")]
        [SerializeField] private RectTransform finishCurrencyTransform = default;

        [SerializeField] private RectTransform simpleElementsRoot = default;
        [SerializeField] private RectTransform passElementsRoot = default;
        [SerializeField] private RectTransform levelElementsRoot = default;
        [SerializeField] private RectTransform bonusRewardElementRoot = default;

        [Header("Lines")]
        [SerializeField] private UiSeasonEventLockLine uiSeasonEventLockLine = default;
        [SerializeField] private VerticalLayoutGroup centralLineLayout = default;

        [Header("Fx")]
        [SerializeField] private IdleEffect[] idleEffects = default;

        [Header("Safe area")]
        [SerializeField] private RectTransform headerSafeAreaRoot = default;

        [Header("Tutorial")]
        [SerializeField] private Image fadeImage = default;

        [Header("Happy hours.")]
        [SerializeField] private SkeletonGraphic timerSkeletonGraphic = default;
        [SerializeField] private UiLiveOpsEvent[] uiLiveOpsEvents = default;

        private SeasonEventProposeController controller;
        private LiveOpsEventController eventController;

        private readonly List<UiSeasonEventRewardElement> elementsToAnimate = new List<UiSeasonEventRewardElement>();

        private readonly Dictionary<SeasonEventRewardType, UiSeasonEventRewardElement[]> allRewardElements =
            new Dictionary<SeasonEventRewardType, UiSeasonEventRewardElement[]>();

        private readonly List<UiSeasonEventRewardElement> allUiElements = new List<UiSeasonEventRewardElement>();

        private readonly List<UiSeasonEventLevelElement> levelElements = new List<UiSeasonEventLevelElement>();

        private float rewardElementHeight;

        private RubberScrollHelper rubberScrollHelper;
        private ScrollRectButtonsHolder scrollRectButtonsHolder;
        private Coroutine uiRefreshRoutine;

        public Action onCloseScreen;

        private UiOverlayTutorialHelper uiTutorial;
        private TrailAnimationHelper trailAnimationHelper;

        #endregion



        #region RewardReceiveScreen

        public override string RewardScreenIdleFxKey =>
            EffectKeys.FxGUIMonopolyRewardOpenShine;

        public override Vector3 GetCurrencyFinishPosition(CurrencyType currencyType) =>
            finishCurrencyTransform.position;

        public override Transform GetCurrencyBounceRoot(CurrencyType currencyType) =>
            finishCurrencyTransform;

        #endregion



        #region Properties

        public override ScreenType ScreenType =>
            ScreenType.SeasonEventScreen;


        private int FirstNotReachedElement
        {
            get
            {
                UiSeasonEventRewardElement firstNotReachedElement = allUiElements
                                    .Where(e => !controller.IsNotSequenceSeasonEventRewardType(e.SeasonEventRewardType))
                                    .OrderBy(DefineLocalIndex)
                                    .ToList()
                                    .Find(e => !controller.IsRewardReached(DefineLocalIndex(e), e.SeasonEventRewardType));

                return DefineLocalIndex(firstNotReachedElement);
            }
        }

        #endregion



        #region Methods

        public override void Initialize(Action<AnimatorView> onShowEndCallback = null,
                                        Action<AnimatorView> onHideEndCallback = null,
                                        Action<AnimatorView> onShowBeginCallback = null,
                                        Action<AnimatorView> onHideBeginCallback = null)
        {
            base.Initialize(onShowEndCallback, onHideEndCallback, onShowBeginCallback, onHideBeginCallback);

            controller = GameServices.Instance.ProposalService.SeasonEventProposeController;
            timerSkeletonGraphic.Initialize(false);

            SetReward(SeasonEventRewardType.Simple, simpleElementsRoot);
            SetReward(SeasonEventRewardType.Pass, passElementsRoot);
            SetReward(SeasonEventRewardType.Bonus, bonusRewardElementRoot);
            SetReward(SeasonEventRewardType.Main, bonusRewardElementRoot);

            SetLevelsElements();

            rewardElementHeight = levelElements.IsNullOrEmpty() ?
                default : (levelElements.First().transform as RectTransform).rect.height + centralLineLayout.spacing;

            uiSeasonEventLockLine.Initialize(rewardElementHeight, 27.0f);

            RefreshElementsState(true);

            VisitAllElements(element =>
            {
                int localIndex = DefineLocalIndex(element);

                if (controller.ShouldAnimateReward(localIndex, element.SeasonEventRewardType))
                {
                    elementsToAnimate.Add(element);
                    levelElements[localIndex].SetState(false, true);
                }

                element.OnShouldReceiveReward += Element_OnShouldReceiveReward;
            });

            controller.OnFinished += Controller_OnLiveOpsFinished;
            MonoBehaviourLifecycle.OnUpdate += MonoBehaviourLifecycle_OnUpdateRefreshTimer;

            foreach (var data in petSkinsData)
            {
                bool shouldShowRoots = controller.IsPetMainReward &&
                                       controller.PetMainRewardType == data.skinType;

                foreach (var r in data.roots)
                {
                    CommonUtility.SetObjectActive(r, shouldShowRoots);
                }
            }

            if (controller.IsPetMainReward)
            {
                petSkeletonGraphic.skeletonDataAsset = IngameData.Settings.pets.uiSettings.FindMainMenuSkeletonData(controller.PetMainRewardType);
                petSkeletonGraphic.Initialize(true);
            }

            CommonUtility.SetObjectsActive(petRewardRoot, controller.IsPetMainReward);
            CommonUtility.SetObjectsActive(commonRewardRoot, !controller.IsPetMainReward);

            RefreshButtonsUi();

            uiSeasonEventBar.Initialize();

            eventController = GameServices.Instance.ProposalService.HappyHoursSeasonEventProposeController;
            foreach (var uiLiveOpsEvent in uiLiveOpsEvents)
            {
                uiLiveOpsEvent.Initialize(eventController);
                uiLiveOpsEvent.SetForceProposePlaceAllowed(false);
            }
        }


        public override void Deinitialize()
        {
            controller.OnFinished -= Controller_OnLiveOpsFinished;
            MonoBehaviourLifecycle.OnUpdate -= MonoBehaviourLifecycle_OnUpdateRefreshTimer;

            MonoBehaviourLifecycle.StopPlayingCorotine(uiRefreshRoutine);

            DOTween.Kill(this);
            DOTween.Kill(scrollEventTriggers);

            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
            Scheduler.Instance.UnscheduleAllMethodForTarget(uiTutorial);

            VisitAllElements(e =>
            {
                e.Deinitialize();
                e.OnShouldReceiveReward -= Element_OnShouldReceiveReward;
                e.OnShouldReceiveReward -= Element_OnShouldStopTutorial;
            });

            foreach (var d in allRewardElements)
            {
                foreach (var element in d.Value)
                {
                    // element.Deinitialize();
                    Content.Management.DestroyUiSeasonEventRewardElement(element);
                }
            }

            allRewardElements.Clear();

            foreach (var element in levelElements)
            {
                element.Deinitialize();
                Content.Management.DestroyUiSeasonEventLevelElement(element);
            }

            levelElements.Clear();

            uiSeasonEventBar.Deinitialize();
            uiSeasonEventLockLine.Deinitialize();

            scrollRectButtonsHolder.Deinitialize();

            uiTutorial?.Deinitialize();

            foreach (var uiLiveOpsEvent in uiLiveOpsEvents)
            {
                uiLiveOpsEvent.Deinitialize();
            }

            trailAnimationHelper?.Deinitialize();

            StopAllCoroutines();
            StopIdleFxs();

            base.Deinitialize();
        }


        public override void Show()
        {
            base.Show();

            Rect mainCanvasRect = mainCanvas.GetComponent<RectTransform>().rect;
            Vector2 saveTopOffset = new Vector2(headerSafeAreaRoot.anchorMax.x, headerSafeAreaRoot.anchorMax.y - SafeOffset.GetSafeTopRatio(mainCanvasRect));
            headerSafeAreaRoot.anchorMax = saveTopOffset;
            headerSafeAreaRoot.anchoredPosition = Vector2.zero;

            rubberScrollHelper = new RubberScrollHelper(scrollRect, mainCanvas, scrollEventTriggers);

            IScrollButton[] buttons = allUiElements.OfType<IScrollButton>().ToArray();
            
            scrollRectButtonsHolder = new ScrollRectButtonsHolder(rubberScrollHelper, buttons);
            scrollRectButtonsHolder.Initialize();

            uiTutorial = new UiOverlayTutorialHelper(fadeImage, controller.Settings.tutorialFadeAnimation, true);
            uiTutorial.Initialize();

            RefreshButtonsUi();

            int firstNotReachedIndex = FirstNotReachedElement;
            int rewardIndexForLockLine;

            if (elementsToAnimate.IsNullOrEmpty())
            {
                rewardIndexForLockLine = firstNotReachedIndex;
            }
            else
            {
                bool isOnlyNotSequence = elementsToAnimate.Count(e => controller.IsNotSequenceSeasonEventRewardType(e.SeasonEventRewardType)) == elementsToAnimate.Count;
                
                rewardIndexForLockLine = isOnlyNotSequence ? 
                    controller.MaxLevelIndexWithoutMainAndBonus + 1 : 
                    elementsToAnimate.Where(e => !controller.IsNotSequenceSeasonEventRewardType(e.SeasonEventRewardType)).Select(DefineLocalIndex).OrderBy(e => e).First();
            }

            SetLockLinePosition(rewardIndexForLockLine, true);

            int rewardIndexForScrolling = GetIndexForScroll();
            
            uiRefreshRoutine = MonoBehaviourLifecycle.PlayCoroutine(OnUiRefreshed(rewardIndexForScrolling));

            foreach (var idleEffect in idleEffects)
            {
                idleEffect.CreateAndPlayEffect();
            }

            RefreshHeaderText();

            controller.ShouldShowLevelFinishAlert = false;

            if (controller.IsSeasonPassActive)
            {
                getSeasonPassButton.onClick.RemoveListener(GetSeasonPassButton_OnClick);
            }

            int GetIndexForScroll()
            {
                int result = default;

                if (!elementsToAnimate.IsNullOrEmpty())
                {
                    bool isOnlyNotSequence = !elementsToAnimate.Contains(e => e.SeasonEventRewardType == SeasonEventRewardType.Simple ||
                                                                    e.SeasonEventRewardType == SeasonEventRewardType.Pass);

                    bool isBonus = elementsToAnimate.Contains(e => controller.IsNotSequenceSeasonEventRewardType(e.SeasonEventRewardType));
                    result = (isOnlyNotSequence && isBonus) ? controller.MaxLevelIndexWithoutMainAndBonus + 1 : DefineLocalIndex(elementsToAnimate.OrderBy(e => e.SeasonEventRewardType).First()) - 1;
                }
                else
                {
                    bool isNotReachedExists = firstNotReachedIndex != -1;

                    if (isNotReachedExists)
                    {
                        result = Mathf.Max(0, firstNotReachedIndex - 2);
                    }
                    else
                    {
                        result = allUiElements
                                        .Select(DefineLocalIndex)
                                        .OrderBy(e => e)
                                        .Last();
                    }
                }

                return result;
            }
        }


        public override void InitializeButtons()
        {
            closeButton.onClick.AddListener(CloseButton_OnClick);
            infoButton.onClick.AddListener(InfoButton_OnClick);
            getSeasonPassButton.onClick.AddListener(GetSeasonPassButton_OnClick);

            VisitAllElements(e => e.InitializeButtons());
        }


        public override void DeinitializeButtons()
        {
            closeButton.onClick.RemoveListener(CloseButton_OnClick);
            infoButton.onClick.RemoveListener(InfoButton_OnClick);
            getSeasonPassButton.onClick.RemoveListener(GetSeasonPassButton_OnClick);

            VisitAllElements(e => e.DeinitializeButtons());
        }


        // also used in Unity Animation Event
        private void RefreshButtonsUi()
        {
            bool isIAPsAvailable = GameServices.Instance.CommonStatisticService.IsIapsAvailable;

            CommonUtility.SetObjectActive(getSeasonPassButton.gameObject, isIAPsAvailable);
            CommonUtility.SetObjectActive(getSeasonPassRootCommon, isIAPsAvailable && !controller.IsSeasonPassActive);
            CommonUtility.SetObjectActive(getSeasonPassRootPurchased, isIAPsAvailable && controller.IsSeasonPassActive);
        }


        private void SetReward(SeasonEventRewardType seasonEventRewardType, Transform root)
        {
            allRewardElements.Add(seasonEventRewardType, Array.Empty<UiSeasonEventRewardElement>());

            RewardData[] reward = controller.GetGeneratedReward(seasonEventRewardType);

            List<UiSeasonEventRewardElement> elements = new List<UiSeasonEventRewardElement>(reward.Length);
            for (int i = 0; i < reward.Length; i++)
            {
                UiSeasonEventRewardElement elementToAdd = Content.Management.CreateUiSeasonEventRewardElement(seasonEventRewardType, root);
                elementToAdd.SetupReward(reward[i]);
                elementToAdd.SetupIndex(i);
                elementToAdd.Initialize(controller);

                elements.Add(elementToAdd);
            }

            allRewardElements[seasonEventRewardType] = elements.ToArray();
            allUiElements.AddRange(elements);
        }


        private void SetLevelsElements()
        {
            for (int i = 0; i <= controller.MaxLevelIndexWithoutMainAndBonus; i++)
            {
                UiSeasonEventLevelElement elementToAdd = Content.Management.CreateUiSeasonEventLevelElement(levelElementsRoot);

                bool isRewardReached = controller.IsRewardReached(i, SeasonEventRewardType.Simple);
                elementToAdd.SetupNumber(i, i < controller.MaxLevelIndexWithoutMainAndBonus);
                elementToAdd.Initialize();

                elementToAdd.SetState(isRewardReached, true);

                levelElements.Add(elementToAdd);
            }
        }


        private int DefineLocalIndex(UiSeasonEventRewardElement uiSeasonEventRewardElement)
        {
            if (uiSeasonEventRewardElement == null || !allRewardElements.ContainsKey(uiSeasonEventRewardElement.SeasonEventRewardType))
            {
                return -1;
            }
            var elements = allRewardElements[uiSeasonEventRewardElement.SeasonEventRewardType];
            int result = Array.IndexOf(elements, uiSeasonEventRewardElement);

            return result;
        }


        public override void OnCurrencyRootScaledIn()
        {
            base.OnCurrencyRootScaledIn();

            EffectManager.Instance.PlaySystemOnce(EffectKeys.FxGUISeasonPassCounterUp,
                finishCurrencyTransform.position,
                finishCurrencyTransform.rotation,
                finishCurrencyTransform);
        }


        // Also called from unity animator
        private void StopIdleFxs()
        {
            foreach (var idleEffect in idleEffects)
            {
                idleEffect.StopEffect();
            }
        }


        // dirty method
        private void SetLockLinePosition(int animateIndex, 
            bool isImmediately, bool isMainRewardAnimation = false, bool tutorial = false)
        {
            int indexToCheck = animateIndex == -1 ? controller.MaxLevelIndexWithoutMainAndBonus + 1 : animateIndex;
            float elementPositionY = uiSeasonEventLockLine.CalculateTargetPositionY(indexToCheck);

            if (indexToCheck > controller.MaxLevelIndexWithoutMainAndBonus)
            {
                bool wasMainAnimatedOnce = controller.WasRewardAnimatedOnce(SeasonEventRewardType.Main);
                if ((isMainRewardAnimation && !isImmediately) || wasMainAnimatedOnce)
                {
                    // Also add 1000.0f to ensure that user won't scroll enough to see lock line background (infinity move out imitation)
                    elementPositionY = Mathf.Max(elementPositionY, Screen.height) + bonusRewardElementRoot.rect.height + 1000.0f;
                }
                else
                {
                    const float visualCorrectOffset = 56.0f; // just visual non calculation additional offset
                    elementPositionY += visualCorrectOffset;
                }
            }

            string showKey = AnimationKeys.SeasonEvent.ShowLockLine;
            string hideKey = tutorial ? AnimationKeys.SeasonEvent.ShortHideLockLine : AnimationKeys.SeasonEvent.HideLockLine;

            uiSeasonEventLockLine.SetPosition(elementPositionY, isImmediately, showKey, hideKey);
        }


        private void VisitAllElements(Action<UiSeasonEventRewardElement> callback)
        {
            foreach (var element in allUiElements)
            {
                callback?.Invoke(element);
            }
        }
        

        private void RefreshHeaderText()
        {
            string key = controller.IsPetMainReward ? controller.VisualSettings.FindHeaderKey(controller.PetMainRewardType) : controller.VisualSettings.commonRewardHeaderKey;

            headerText.SetTerm(key);
        }

        #endregion



        #region Events handlers

        public override Vector3 GetCurrencyStartPosition(RewardData rewardData)
        {
            UiSeasonEventRewardElement foundElement = allUiElements.Find(e => e.RewardData == rewardData);

            if (foundElement == null)
            {
                CustomDebug.Log($"Can't find currency icon transform in {this}");
            }

            return foundElement == null ? default : foundElement.IconCurrencyTransform.position;
        }


        private void CloseButton_OnClick()
        {
            if (!elementsToAnimate.IsNullOrEmpty())
            {
                return;
            }

            DeinitializeButtons();

            FromLevelToLevel.PlayTransition(() =>
            {
                HideImmediately();

                if (onCloseScreen == null)
                {
                    LevelsManager.Instance.UnloadLevel();
                    LevelsManager.Instance.LoadScene(GameServices.Instance.PlayerStatisticService.PlayerData.LastPlayedMode, GameMode.MenuScene);
                    UiScreenManager.Instance.ShowScreen(ScreenType.MainMenu, isForceHideIfExist: true);
                }
                else
                {
                    onCloseScreen?.Invoke();
                }
            });
        }


        private void InfoButton_OnClick() =>
            UiScreenManager.Instance.ShowScreen(ScreenType.SeasonEventInfo, isForceHideIfExist: true);


        private void GetSeasonPassButton_OnClick()
        {
            if (controller.IsSeasonPassActive)
            {
                CustomDebug.Log("Season pass is already active!");
                return;
            }

            controller.OnFinished -= Controller_OnLiveOpsFinished;

            var offer = GameServices.Instance.ProposalService.GetOffer<GoldenTicketOffer>();
            offer.ForcePropose(OfferKeys.EntryPoint.Season, () =>
            {
                if (controller.IsSeasonPassActive)
                {
                    controller.OnFinished += Controller_OnLiveOpsFinished;

                    RefreshButtonsUi();
                    RefreshElementsState(false);

                    getSeasonPassButton.onClick.RemoveListener(GetSeasonPassButton_OnClick);
                }
            });
        }


        private void Controller_OnLiveOpsFinished()
        {
            DeinitializeButtons();

            FromLevelToLevel.PlayTransition(() =>
            {
                UiScreenManager.Instance.HideAll(true);

                LevelsManager.Instance.UnloadLevel();
                LevelsManager.Instance.LoadScene(GameServices.Instance.PlayerStatisticService.PlayerData.LastPlayedMode, GameMode.MenuScene);
                UiScreenManager.Instance.ShowScreen(ScreenType.MainMenu, isForceHideIfExist: true);
            });
        }


        private void MonoBehaviourLifecycle_OnUpdateRefreshTimer(float deltaTime)
        {
            timeLeftText.text = controller.TimeLeftLiveOpsUi;

            timerSkeletonGraphic.Skeleton.SetSkin(eventController.UiTimerSkeletonGraphicSkin);
        }


        private void Element_OnShouldReceiveReward(UiSeasonEventRewardElement element)
        {
            element.PlayClaimFx();

            if (element.RewardData.Type == RewardType.SpinRouletteCash ||
                element.RewardData.Type == RewardType.SpinRouletteSkin ||
                element.RewardData.Type == RewardType.SpinRouletteWaipon ||
                element.RewardData.Type == RewardType.Forcemeter)
            {
                DeinitializeButtons();
            }

            OnShouldApplyReward(element.RewardData);

            RefreshElementsState(false);
            RefreshBar();
        }


        private void Element_OnShouldStopTutorial(UiSeasonEventRewardElement element)
        {
            Scheduler.Instance.UnscheduleAllMethodForTarget(uiTutorial);
            scrollRect.enabled = true;
            uiTutorial.StopTutorial(() => StartCoroutine(RefreshRectMask()));

            VisitAllElements((e) =>
            {
                e.SetButtonAnimationEnabled(true);
                e.SetTutorialEnabled(false);
                e.OnShouldReceiveReward -= Element_OnShouldStopTutorial;
            });


            IEnumerator RefreshRectMask()
            {
                yield return new WaitForEndOfFrame();
                rectMask2D.enabled = !rectMask2D.enabled;
                rectMask2D.enabled = !rectMask2D.enabled;
            }
        }


        #warning too expensive method
        private void RefreshElementsState(bool isImmediately)
        {
            VisitAllElements((element) =>
            {
                int localIndex = DefineLocalIndex(element);
                RewardElementState state = controller.DefineElementInitialState(element.SeasonEventRewardType, localIndex);

                element.SetState(state, isImmediately);
            });
        }


        private void RefreshBar()
        {
            float currentCurrency = GameServices.Instance.PlayerStatisticService.CurrencyData.GetEarnedCurrency(CurrencyType.SeasonEventPoints);
            uiSeasonEventBar.PlayAnimation(controller.PointsCountOnPreviousShow, currentCurrency, OnLastLapFilled, OnLastLapFilled);
            controller.PointsCountOnPreviousShow = currentCurrency;
        }


        private IEnumerator OnUiRefreshed(int rewardIndexForScrolling)
        {
            RefreshBar();
            
            yield return new WaitForEndOfFrame();

            Vector2 additionalContentExpandHeight = Vector2.zero.SetY(centralLineLayout.padding.top + bonusRewardElementRoot.rect.height);
            rubberScrollHelper.ExpandContent(centralLineLayout, additionalContentExpandHeight);
            uiSeasonEventLockLine.SetupLockBackgroundHeight(scrollRect.content.sizeDelta.y);

            float contentOffset = scrollRect.content.rect.height * scrollRect.content.pivot.y;

            float targetOffset = -(rewardIndexForScrolling * rewardElementHeight) + centralLineLayout.padding.top + centralLineLayout.padding.bottom;
            float targetPositionY = Mathf.Min(scrollRect.content.rect.height, contentOffset + targetOffset);
            scrollRect.content.anchoredPosition = scrollRect.content.anchoredPosition.SetY(targetPositionY);
        }


        private void OnLastLapFilled()
        {
            bool shouldAnimate = !elementsToAnimate.IsNullOrEmpty();

            if (!shouldAnimate)
            {
                return;
            }

            bool shouldPlayTutorial = controller.ShouldPlayTutorialAnimation;
            bool isMainReward = elementsToAnimate.Contains(e => e.SeasonEventRewardType == SeasonEventRewardType.Main);
            bool isAllBonusRewards = elementsToAnimate.All(e => e.SeasonEventRewardType == SeasonEventRewardType.Bonus);

            uiSeasonEventBar.Pause();

            if (shouldPlayTutorial || isAllBonusRewards)
            {
                uiSeasonEventLockLine.SetFxEnabled(false);
                Scheduler.Instance.CallMethodWithDelay(this, OnTrailReached, CommonUtility.OneFrameDelay); // hotfix cuz of scheduler and unity animation simultanious delays
            }
            else
            {
                Guid trailSfx = SoundManager.Instance.PlaySound(AudioKeys.Ui.WHEEL_WIN_FLYING_COLLECT, isLooping: true);

                EffectManager.Instance.PlaySystemOnce(EffectKeys.FxGUISeasonPassRhombBarBeforeTrail, uiSeasonEventBar.LevelBackImage.transform.position, uiSeasonEventBar.LevelBackImage.rotation, uiSeasonEventBar.LevelBackImage);
                trailAnimationHelper = new TrailAnimationHelper(controller.VisualSettings.barTrailAnimation, EffectKeys.FxGUISeasonPassRewardTrail, transform);
                trailAnimationHelper.Initialize();

                const float trailHideDelay = 0.4f;
                trailAnimationHelper.PlayTrail(uiSeasonEventBar.LevelBackImage.transform.position, uiSeasonEventLockLine.IconLockRoot.transform.position, () =>
                {
                    OnTrailReached();
                    Scheduler.Instance.CallMethodWithDelay(this, () => SoundManager.Instance.StopSound(trailSfx), trailHideDelay);
                }, trailHideDelay);
            }


            void OnTrailReached()
            {
                if (shouldPlayTutorial)
                {
                    scrollRect.enabled = false;
                    UiSeasonEventRewardElement[] tutorialObjects = elementsToAnimate.Where(e =>
                        e.SeasonEventRewardType == SeasonEventRewardType.Simple ||
                        e.SeasonEventRewardType == SeasonEventRewardType.Pass).ToArray();

                    Scheduler.Instance.CallMethodWithDelay(this, () =>
                    {
                        uiTutorial.SetupOverlayedObjects(tutorialObjects);
                        uiTutorial.PlayTutorial();

                        foreach (var o in tutorialObjects)
                        {
                            o.RefreshText();
                        }
                    }, controller.VisualSettings.tutorialStartDelayAfterBarFilled);
                }

                bool shouldUseShortHide = shouldPlayTutorial || isAllBonusRewards;
                float elementsStateAnimationDelayAfterBarFilled = controller.VisualSettings.GetElementsStateAnimationDelayAfterBarFilled(shouldUseShortHide);
                Scheduler.Instance.CallMethodWithDelay(this, () =>
                {
                    for (int i = 0; i < elementsToAnimate.Count; i++)
                    {
                        UiSeasonEventRewardElement e = elementsToAnimate[i];

                        if (shouldPlayTutorial)
                        {
                            e.SetButtonAnimationEnabled(e.SeasonEventRewardType == SeasonEventRewardType.Simple ||
                                                        e.SeasonEventRewardType == SeasonEventRewardType.Pass);
                            Scheduler.Instance.CallMethodWithDelay(uiTutorial, () => e.SetTutorialEnabled(true),
                                i * controller.VisualSettings.rewardElementTutorialDelay);
                            e.OnShouldReceiveReward += Element_OnShouldStopTutorial;
                        }
                    }

                    controller.MarkAllReachedRewardAnimated();

                    List<UiSeasonEventRewardElement> savedElementsToAnimate = new List<UiSeasonEventRewardElement>(elementsToAnimate);
                    elementsToAnimate.Clear();

                    RefreshElementsState(false);

                    savedElementsToAnimate = savedElementsToAnimate.Where(e => e.CurrentState == RewardElementState.ReadyToClaim).ToList();
                    foreach (var e in savedElementsToAnimate)
                    {
                        e.PlayLockDestroyFx();
                    }

                    int levelElementsRechedIndex = controller.LevelReachIndex();
                    if (CommonUtility.IsIndexCorrect(levelElements, levelElementsRechedIndex))
                    {
                        levelElements[levelElementsRechedIndex].SetState(true, false);
                    }

                    if (CommonUtility.IsIndexCorrect(levelElements, levelElementsRechedIndex + 1))
                    {
                        levelElements[levelElementsRechedIndex + 1].PlayShineAnimation();
                    }

                }, elementsStateAnimationDelayAfterBarFilled);

                int firstNotReachedIndex = FirstNotReachedElement;
                float lockLineAnimationDelayAfterBarFilled = controller.VisualSettings.GetLockLineAnimationDelayAfterBarFilled(shouldUseShortHide);
                Scheduler.Instance.CallMethodWithDelay(this, () => SetLockLinePosition(firstNotReachedIndex, false, isMainReward, shouldPlayTutorial),
                    lockLineAnimationDelayAfterBarFilled);

                float progressBarResumeDelayAfterBarFilled = controller.VisualSettings.GetProgressBarResumeDelayAfterBarFilled(shouldUseShortHide);
                Scheduler.Instance.CallMethodWithDelay(this, () =>
               {
                   uiSeasonEventLockLine.SetFxEnabled(true);
                   uiSeasonEventBar.Resume();
               }, progressBarResumeDelayAfterBarFilled);
            }
        }

        #endregion
    }
}
