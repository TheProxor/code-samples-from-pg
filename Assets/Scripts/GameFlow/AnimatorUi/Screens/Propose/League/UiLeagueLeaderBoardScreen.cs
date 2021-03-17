using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using Drawmasters.Levels;
using Drawmasters.ServiceUtil;
using Drawmasters.Utils;
using Drawmasters.Effects;
using Drawmasters.Utils.Ui;
using Drawmasters.Proposal;
using Drawmasters.Proposal.Ui;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Modules.General;
using DG.Tweening;
using I2.Loc;
using Modules.Sound;


namespace Drawmasters.Ui
{
    public class UiLeagueLeaderBoardScreen : AnimatorScreen
    {
        #region Nested types

        [Serializable]
        private class VisualData
        {
            public LeagueType leagueType = default;
            public GameObject[] roots = default;
        }

        #endregion



        #region Fields

        [SerializeField] private VisualData[] visualData = default;
        [SerializeField] private IdleEffect[] idleEffects = default;

        [SerializeField] private Button closeButton = default;
        [SerializeField] private Button infoButton = default;

        [Header("Top Header")]
        [SerializeField] private Localize leagueLocalizeText = default;

        [Header("Scroll")]
        [SerializeField] private RubberScrollRect rubberScrollRect = default;
        [SerializeField] private EventTrigger eventTrigger = default;
        [SerializeField] private UiLeagueLeaderBoardRewardRootMain mainReward = default;

        [Header("Bottom")]
        [SerializeField] private SkeletonGraphic timerSkeletonGraphic = default;
        [SerializeField] private TMP_Text timerText = default;

        [Header("Happy hours")]
        [SerializeField] private UiLiveOpsEvent[] uiLiveOpsEvents = default;

        [Header("Separator")]
        [SerializeField] private RectTransform leagueSeparator = default;
        [SerializeField] private RectTransform nextLeagueBackgroundFade = default;
        [SerializeField] private RectTransform nextLeagueBackground = default;

        [SerializeField] private RectTransform nextLeagueBackgroundFadeNoStencil = default;
        [SerializeField] private RectTransform nextLeagueBackgroundNoStencil = default;

        [Header("Intermediate Reward")]
        [SerializeField] private UiLeagueIntermediateRewardProgressBar intermediateRewardProgressBar = default;

        private LiveOpsEventController eventController;

        private LoopedInvokeTimer timeLeftRefreshTimer;
        private LeagueProposeController controller;
        private RubberScrollHelper rubberScrollHelper;
        private UiLeagueLeaderBoardElementScrollLocker scrollLocker;
        private ScrollElementsMoveUtility scrollElementsMoveUtility;

        private RectTransform rubberScrollRectTransform;

        private List<UiLeagueLeaderBoardElement> elements = new List<UiLeagueLeaderBoardElement>();

        private Coroutine scrollRoutine;

        public Action onCloseScreen;

        private (RectTransform fade, RectTransform back)[] nextLeagueRects;

        private LoopedInvokeTimer leaderboardRefreshTimer;

        #endregion



        #region Properties

        public override ScreenType ScreenType =>
            ScreenType.LeagueLeaderBoard;

        #endregion



        #region Overrided methods

        public override void Initialize(Action<AnimatorView> onShowEndCallback = null,
                                        Action<AnimatorView> onHideEndCallback = null,
                                        Action<AnimatorView> onShowBeginCallback = null,
                                        Action<AnimatorView> onHideBeginCallback = null)
        {
            base.Initialize(onShowEndCallback, onHideEndCallback, onShowBeginCallback, onHideBeginCallback);

            rubberScrollRectTransform = rubberScrollRect.transform as RectTransform;

            foreach (var idleEffect in idleEffects)
            {
                idleEffect.CreateAndPlayEffect();
            }

            nextLeagueRects = new[] { (nextLeagueBackgroundFade, nextLeagueBackground),
                                      (nextLeagueBackgroundFadeNoStencil, nextLeagueBackgroundNoStencil) };

            MonoBehaviourLifecycle.OnUpdate += RefreshNextLeagueBackground_OnUpdate;

            intermediateRewardProgressBar.Initialize();

        }


        public override void Deinitialize()
        {
            MonoBehaviourLifecycle.OnUpdate -= RefreshNextLeagueBackground_OnUpdate;

            foreach (var element in elements)
            {
                element.Deinitialize();
                Content.Management.DestroyUiLeagueLeaderBoardElement(element);
            }

            mainReward.Deintiailize();
            intermediateRewardProgressBar.Deinitialize();

            elements.Clear();

            foreach (var idleEffect in idleEffects)
            {
                idleEffect.StopEffect();
            }

            scrollLocker?.Deinitialize();

            scrollElementsMoveUtility?.Deinitialize();

            if (controller != null)
            {
                controller.OnFinished -= Controller_OnLiveOpsFinished;
            }

            foreach (var uiLiveOpsEvent in uiLiveOpsEvents)
            {
                uiLiveOpsEvent.Deinitialize();
            }

            leaderboardRefreshTimer?.Stop();

            DOTween.Kill(this);
            DOTween.Kill(rubberScrollHelper);
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
            MonoBehaviourLifecycle.StopPlayingCorotine(scrollRoutine);

            timeLeftRefreshTimer?.Stop();

            controller.LeaderBoard.PlayerData.WasPlayerElementAnimatedOnStart = true;

            base.Deinitialize();
        }


        public override void Show()
        {
            base.Show();

            rubberScrollHelper = new RubberScrollHelper(rubberScrollRect, mainCanvas, eventTrigger);

            //TODO HACK
            if (timeLeftRefreshTimer != null && timeLeftRefreshTimer.IsTimerActive)
            {
                timeLeftRefreshTimer.Stop();
            }
            timeLeftRefreshTimer = new LoopedInvokeTimer(RefreshTimeLeft);
            timeLeftRefreshTimer.Start();

            eventController = GameServices.Instance.ProposalService.HappyHoursLeagueProposeController;
            foreach (var uiLiveOpsEvent in uiLiveOpsEvents)
            {
                uiLiveOpsEvent.Initialize(eventController);
                uiLiveOpsEvent.SetForceProposePlaceAllowed(false);
            }
        }


        public override void InitializeButtons()
        {
            closeButton.onClick.AddListener(CloseButton_OnClick);
            infoButton.onClick.AddListener(InfoButton_OnClick);
        }


        public override void DeinitializeButtons()
        {
            closeButton.onClick.RemoveListener(CloseButton_OnClick);
            infoButton.onClick.RemoveListener(InfoButton_OnClick);
        }

        #endregion



        #region Methods

        public void SetupController(LeagueProposeController _controller)
        {
            controller = _controller;
            controller.OnFinished += Controller_OnLiveOpsFinished;
            leaderboardRefreshTimer = new LoopedInvokeTimer(() => RefreshLeaderBoard(false), controller.Settings.leaderBoardRefreshPeriod);
            leaderboardRefreshTimer.Start();

            RefreshVisual();
        }


        public void FillPlayers()
        {
            LeaderBoardItem[] items = controller.LeaderBoard.Items;
            for (int i = 0; i < items.Length; i++)
            {
                UiLeagueLeaderBoardElement elementToAdd = Content.Management.CreateUiLeagueLeaderBoardElement(rubberScrollRect.content);
                elements.Add(elementToAdd);
            }

            RefreshLeaderBoard(true);

            foreach (var element in elements)
            {
                element.Initialize();
            }

            RefreshMainRewardData();
        }


        public void InitialScroll()
        {
            scrollRoutine = MonoBehaviourLifecycle.PlayCoroutine(PerfomAction());

            IEnumerator PerfomAction()
            {
                yield return new WaitForEndOfFrame();

                PlayerLeaderBoardData boardPlayer = controller.LeaderBoard.PlayerData;
                UiLeagueLeaderBoardElement player = elements.Find(e => e.CurrentOwnerType == LeaderBordItemType.Player);

                scrollLocker = scrollLocker ?? new UiLeagueLeaderBoardElementScrollLocker(player, rubberScrollHelper);
                scrollLocker.Initialize();

                // hack
                player.SetupSkullsCount(boardPlayer.SkullsCount);
                int targetPlayerSiblingIndex = GetIndexPosition(player);
                int targetPlayerSiblingIndexAnimation = GetIndexPositionForAnimation(player);
                player.SetupSkullsCount(controller.LeaderBoard.SkullsCountOnPreviousLeaderboardShow);

                bool shouldAnimatePlayerMovement = targetPlayerSiblingIndex != controller.LeaderBoard.PreviousLeaderBoardPosition;
                if (shouldAnimatePlayerMovement)
                {
                    // moving hack not to change core scroll animation. It's easier to change separator position firstly (GD aproved)
                    bool isMovingUnderLeague = controller.LeaderBoard.PreviousLeaderBoardAnimationPosition > leagueSeparator.GetSiblingIndex() &&
                                               targetPlayerSiblingIndexAnimation < leagueSeparator.GetSiblingIndex();

                    bool isMovingUpToLeague = controller.LeaderBoard.PreviousLeaderBoardAnimationPosition < leagueSeparator.GetSiblingIndex() &&
                                              targetPlayerSiblingIndexAnimation > leagueSeparator.GetSiblingIndex();
                    int indexOffset = 0;
                    indexOffset = isMovingUnderLeague ? -1 : indexOffset;
                    indexOffset = isMovingUpToLeague ? 1 : indexOffset;

                    SortElements(indexOffset);

                    LeaderBordItemType leaderBordItemType = default;

                    bool shouldPlayMainRewardChangeAnimation = targetPlayerSiblingIndex == 0 ||
                                                               controller.LeaderBoard.PreviousLeaderBoardPosition == 0;
                    if (shouldPlayMainRewardChangeAnimation)
                    {
                        if (controller.LeaderBoard.PreviousLeaderBoardPosition == 0)
                        {
                            leaderBordItemType = LeaderBordItemType.Bot;
                        }
                        else if (targetPlayerSiblingIndex == 0)
                        {
                            leaderBordItemType = LeaderBordItemType.Player;
                        }

                        mainReward.SetupOwnerType(leaderBordItemType);
                        mainReward.PlayGraphicChangeAnimation();
                    }

                    leaderboardRefreshTimer.Stop();
                    scrollLocker.StartChecking();

                    controller.VisualSettings.playerElementMoveScaleAnimation.Play(value =>
                        player.transform.localScale = value, this);

                    Scheduler.Instance.CallMethodWithDelay(this, () =>
                    {
                        SetScrollPositionToPlayer();

                        int targetPlayerSiblingIndexToMove = targetPlayerSiblingIndexAnimation;
                        scrollElementsMoveUtility = scrollElementsMoveUtility ?? new ScrollElementsMoveUtility(controller.VisualSettings.elementsMoveAnimation, controller.VisualSettings.elementsMoveSizeAnimation);
                        scrollElementsMoveUtility.MoveLayoutElement(rubberScrollRect.content, player.LayoutElement, targetPlayerSiblingIndexToMove, (position) =>
                        {
                            SetScrollPositionToPlayer();
                            RefreshNumbers(e => e.RectTransform.anchoredPosition.y);

                        }, () =>
                        {
                            controller.VisualSettings.playerElementMoveScaleAnimation.Play(value =>
                                player.transform.localScale = value, this, isReversed: true);

                            player.SetupSkullsCount(boardPlayer.SkullsCount);
                            SortElements();
                            scrollLocker.FinishChecking();
                            scrollLocker.RefreshLocking();
                            leaderboardRefreshTimer.Start();

                            PlayScrollStartAnimation();
                        });
                    }, controller.VisualSettings.startElementsMoveDelay);

                    SetScrollPositionToPlayer();

                    void SetScrollPositionToPlayer()
                    {
                        Vector3 playerScrollPosition = GetElementScrollPosition(player);
                        rubberScrollRect.content.anchoredPosition3D = playerScrollPosition;
                    }
                }
                else
                {
                    player.SetupSkullsCount(boardPlayer.SkullsCount);
                    SortElements();

                    Vector3 endScrollPosition = GetElementScrollPosition(player);
                    rubberScrollRect.content.anchoredPosition3D = endScrollPosition;
                    scrollLocker.RefreshLocking();

                    player.SetupSkullsCount(controller.LeaderBoard.SkullsCountOnPreviousLeaderboardShow);

                    PlayScrollStartAnimation();
                }

                controller.VisualSettings.elementsSkullsAnimation.SetupDelay(shouldAnimatePlayerMovement ? controller.VisualSettings.startElementsMoveDelay : 0.0f);
                controller.VisualSettings.elementsSkullsAnimation.SetupBeginValue(controller.LeaderBoard.SkullsCountOnPreviousLeaderboardShow);
                controller.VisualSettings.elementsSkullsAnimation.SetupEndValue(boardPlayer.SkullsCount);
                controller.VisualSettings.elementsSkullsAnimation.Play(value =>
                    player.SetupSkullsCount(value), this);

                controller.LeaderBoard.PreviousLeaderBoardPosition = targetPlayerSiblingIndex;
                controller.LeaderBoard.PreviousLeaderBoardAnimationPosition = targetPlayerSiblingIndexAnimation;
                controller.LeaderBoard.SkullsCountOnPreviousLeaderboardShow = boardPlayer.SkullsCount;

                LayoutRebuilder.ForceRebuildLayoutImmediate(rubberScrollRect.content);

                SetupNextLeagueFade(player);
                RefreshNextLeagueBackground(default);
            }


            void SetupNextLeagueFade(UiLeagueLeaderBoardElement _player)
            {
                foreach (var rects in nextLeagueRects)
                {
                    Vector2 fadeSavedSizeDelta = rects.fade.rect.size;
                    Vector2 fadeSavedPosition = rects.fade.position;

                    int nextLeagueElementsCount = controller.Settings.CountPositionForNextLeagueAchived;
                    float nextLeagueBackgroundY = nextLeagueElementsCount * _player.RectTransform.sizeDelta.y +
                                                  mainReward.RectTransform.sizeDelta.y +
                                                  leagueSeparator.sizeDelta.y +
                                                  RubberScrollRect.AdditionalBackOffset;

                    rects.back.sizeDelta = rects.back.sizeDelta.SetY(nextLeagueBackgroundY);

                    rects.fade.anchorMin = Vector2.one * 0.5f;
                    rects.fade.anchorMax = Vector2.one * 0.5f;
                    rects.fade.position = fadeSavedPosition;
                    rects.fade.sizeDelta = fadeSavedSizeDelta;
                }
            }
        }


        private void PlayScrollStartAnimation()
        {
            if (controller.LeaderBoard.PlayerData.WasPlayerElementAnimatedOnStart)
            {
                return;
            }

            rubberScrollRect.enabled = false;

            scrollLocker.StartChecking();

            rubberScrollHelper.Scroll(rubberScrollHelper.ScrollBottomPosition,
                                      rubberScrollHelper.ScrollTopPosition,
                                      controller.VisualSettings.startScrollAnimation,
                                      rubberScrollHelper,
                                      OnScrolled);

            void OnScrolled()
            {
                scrollLocker.FinishChecking();

                RectTransform[] allElements = GetOrderedContentRectTransforms();

                foreach (var data in controller.VisualSettings.startElementData)
                {
                    RectTransform[] elementsToAnimate = Array.Empty<RectTransform>();

                    foreach (var indexeToAnimate in data.indexesToAnimate)
                    {
                        if (CommonUtility.IsIndexCorrect(allElements, indexeToAnimate))
                        {
                            elementsToAnimate = elementsToAnimate.Add(allElements[indexeToAnimate]);
                        }
                    }

                    data.startAnimation.Play(e =>
                    {
                        foreach (var element in elementsToAnimate)
                        {
                            element.transform.localScale = e;
                        }
                    }, rubberScrollHelper, () =>
                    {
                        data.startAnimation.Play(e =>
                        {
                            foreach (var element in elementsToAnimate)
                            {
                                element.transform.localScale = e;
                            }
                        }, rubberScrollHelper, isReversed: true);
                    });

                    Vector3 fxPosition = elementsToAnimate.FirstOrDefault() == null ? Vector3.zero : elementsToAnimate.FirstOrDefault().transform.position;
                    
                    EffectManager.Instance.PlaySystemOnce(data.onStartFxKey, 
                        fxPosition, 
                        default, 
                        mainCanvas.transform);
                    
                    SoundManager.Instance.PlayOneShot(AudioKeys.Ingame.GRAND_PRIZE);
                }

                rubberScrollRect.enabled = true;
                controller.LeaderBoard.PlayerData.WasPlayerElementAnimatedOnStart = true;
            }
        }


        private Vector3 GetElementScrollPosition(UiLeagueLeaderBoardElement targetElement)
        {
            float endScrollPositionY = rubberScrollHelper.GetSnapPosition(targetElement.RectTransform).y - (rubberScrollRectTransform.rect.height * 0.5f);
            endScrollPositionY = Mathf.Clamp(endScrollPositionY, rubberScrollHelper.ScrollTopPosition.y, ScrollBottomMaxPosition().y);

            Vector3 endScrollPosition = rubberScrollRect.content.anchoredPosition3D.SetY(endScrollPositionY);

            return endScrollPosition;

            Vector3 ScrollBottomMaxPosition()
            {
                Vector3 startPosition = rubberScrollHelper.ScrollTopPosition;

                RectTransform scrollRectTransform = rubberScrollRect.transform as RectTransform;
                float resultY = rubberScrollRect.content.rect.height - scrollRectTransform.rect.height;
                return startPosition.SetY(resultY);
            }
        }


        private void RefreshVisual()
        {
            LeagueType currentLeagueType = controller.LeaderBoard.LeagueType;

            foreach (var data in visualData)
            {
                CommonUtility.SetObjectsActive(data.roots, currentLeagueType == data.leagueType);
            }

            string key = controller.VisualSettings.FindHeaderKey(currentLeagueType);
            leagueLocalizeText.SetTerm(key);

            RefreshTimeLeft();
            RefreshIntermediateReward();
        }


        private void SortElements(int separatorSiblingOffset = 0)
        {
            elements = elements.OrderByDescending(e => e.SkullsCount).ToList();

            for (int i = 0; i < elements.Count; i++)
            {
                elements[i].transform.SetSiblingIndex(i);
            }

            CommonUtility.SetObjectActive(leagueSeparator.gameObject, controller.LeaderBoard.LeagueType.IsNextLeagueAvailable());
            int nextLeagueElementsCount = controller.Settings.CountPositionForNextLeagueAchived;
            leagueSeparator.SetSiblingIndex(nextLeagueElementsCount + separatorSiblingOffset);
            mainReward.RectTransform.SetSiblingIndex(0);

            RefreshNumbers(e => e.SkullsCount);
        }


        private void RefreshNumbers(Func<UiLeagueLeaderBoardElement, float> sortingSelector)
        {
            var sortedElements = elements.OrderByDescending(sortingSelector).ToList();

            for (int i = 0; i < sortedElements.Count; i++)
            {
                bool wasNumberChanged = sortedElements[i].Number != i + 1;

                if (wasNumberChanged)
                {
                    sortedElements[i].SetupNumber(i + 1);
                    sortedElements[i].SetupRewardVisual(controller.LeaderBoard.LeagueType, i, controller.CurrentLiveOpsId);
                }
            }
        }


        private int GetIndexPosition(UiLeagueLeaderBoardElement element) =>
            elements.OrderByDescending(e => e.SkullsCount).ToList().FindIndex(e => e == element);


        private int GetIndexPositionForAnimation(UiLeagueLeaderBoardElement element)
        {
            RectTransform[] allRectTransforms = GetOrderedContentRectTransforms();
            int result = Array.FindIndex(allRectTransforms, e => e == element.RectTransform);
            return result;
        }


        private RectTransform[] GetOrderedContentRectTransforms()
        {
            List<RectTransform> elementRects = elements.OrderByDescending(e => e.SkullsCount).Select(e => e.RectTransform).ToList();
            elementRects.Insert(controller.Settings.CountPositionForNextLeagueAchived, leagueSeparator);
            elementRects.Insert(0, mainReward.RectTransform);

            return elementRects.ToArray();
        }


        private void RefreshMainRewardData()
        {
            mainReward.SetupController(controller);

            UiLeagueLeaderBoardElement firstElement = elements.OrderByDescending(e => e.SkullsCount).FirstOrDefault();
            LeaderBordItemType leaderBordItemType = firstElement == null ? default : firstElement.CurrentOwnerType;
            mainReward.SetupOwnerType(leaderBordItemType);

            RewardData mainRewardData = controller.MainRewardData;
            mainReward.SetupRewardData(mainRewardData);
            mainReward.RefreshVisual();
        }


        private void RefreshIntermediateReward()
        {
            bool isActive = controller.IntermediateRewardController.IsActive;

            intermediateRewardProgressBar.SetObjectActive(controller.IntermediateRewardController.IsActive);
            int earnedRewardsPerStage = controller.IntermediateRewardController.EarnedRewardsPerStage;
            intermediateRewardProgressBar.SetupReceivedRewards(earnedRewardsPerStage);

            if (!isActive)
            {
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
                    sprites.Add(controller.VisualSettings.FindIntermediateLeaderbordChestIcon(chestType));
                }

                intermediateRewardProgressBar.SetChestsVisual(sprites.ToArray());
            }


            void UpdateProgress()
            {
                float pointsCount = controller.IntermediateRewardController.LeaguePointsCount;
                int currentStage = controller.IntermediateRewardController.CurrentStageIndex + 1;
                int stagesCount = controller.IntermediateRewardController.LastStageIndex + 1;

                intermediateRewardProgressBar.RefreshStage(currentStage, stagesCount);
                intermediateRewardProgressBar.UpdateProgress(pointsCount, pointsCount);
            }
        }


        private void RefreshLeaderBoard(bool isFirstShow = false)
        {
            LeaderBoardItem[] items = controller.LeaderBoard.Items.OrderByDescending(e => (isFirstShow && e.ItemType == LeaderBordItemType.Player) ?
                ((PlayerLeaderBoardData)e).SkullsCountOnPreviousLeaderboardShow : e.SkullsCount).ToArray();

            for (int i = 0; i < items.Length; i++)
            {
                string nickName = items[i].NickName;
                LeaderBordItemType itemType = items[i].ItemType;

                float skulls = (isFirstShow && itemType == LeaderBordItemType.Player)
                    ? ((PlayerLeaderBoardData)items[i]).SkullsCountOnPreviousLeaderboardShow
                    : items[i].SkullsCount;
                ShooterSkinType skinType = items[i].SkinType;
                bool isNextLeagueAchived = controller.LeaderBoard.IsNextLeagueAchived(items[i].Id);

                SetupElementData(elements[i], i + 1, nickName, skulls, itemType, skinType, isNextLeagueAchived);
            }

            SortElements();

            UiLeagueLeaderBoardElement player = elements.Find(e => e.CurrentOwnerType == LeaderBordItemType.Player);
            scrollLocker?.SetupElementToMonitor(player);
            scrollLocker?.RefreshLocking();

        }


        private void SetupElementData(UiLeagueLeaderBoardElement element, int number, string nickName,
                                      float skullsCount, LeaderBordItemType ownerType,
                                      ShooterSkinType shooterSkinType, bool isNextLeagueAchieved)
        {
            // TODO: Temp hotfix. It's fine to refresh only with different numbers and skulls, but here come visual errors. To Vladislav.k
            bool shouldRefreshElement = element.Number != number ||
                                        !Mathf.Approximately(element.SkullsCount, skullsCount) ||
                                        !element.NickName.Equals(nickName, StringComparison.Ordinal);
            if (shouldRefreshElement)
            {
                element.SetupController(controller);
                element.SetupOwnerType(ownerType);
                element.SetupLeagueType(controller.LeaderBoard.LeagueType);
                element.SetupNumber(number);
                element.SetupShooterType(shooterSkinType);
                element.SetupNickName(nickName);
                element.SetupSkullsCount(skullsCount);
                element.SetupNextLeagueAchived(isNextLeagueAchieved);
                element.SetupRewardVisual(controller.LeaderBoard.LeagueType, number - 1, controller.CurrentLiveOpsId);
                element.RefreshVisual();
            }
        }

        #endregion



        #region Events handlers

        private void Controller_OnLiveOpsFinished()
        {
            DeinitializeButtons();

            FromLevelToLevel.PlayTransition(() =>
            {
                HideImmediately();

                LevelsManager.Instance.UnloadLevel();
                LevelsManager.Instance.LoadScene(GameServices.Instance.PlayerStatisticService.PlayerData.LastPlayedMode, GameMode.MenuScene);
                UiScreenManager.Instance.ShowScreen(ScreenType.MainMenu, isForceHideIfExist: true);

                GameServices.Instance.MusicService.InstantRefreshMusic();
            });
        }


        private void CloseButton_OnClick()
        {
            DeinitializeButtons();

            FromLevelToLevel.PlayTransition(() =>
            {
                HideImmediately();

                if (onCloseScreen == null)
                {
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
            UiScreenManager.Instance.ShowScreen(ScreenType.LeagueInfo, isForceHideIfExist: true);


        private void RefreshTimeLeft()
        {
            timerText.text = controller.TimeUi;

            timerSkeletonGraphic.Skeleton.SetSkin(eventController.UiTimerSkeletonGraphicSkin);
        }


        private void RefreshNextLeagueBackground(Vector2 pos)
        {
            foreach (var rects in nextLeagueRects)
            {
                Vector3 savedPosition = rects.fade.position;
                Vector2 savedSizeDelta = rects.fade.sizeDelta;

                rects.back.transform.position = leagueSeparator.transform.position;

                rects.fade.position = savedPosition;
                rects.fade.anchoredPosition3D = rects.fade.anchoredPosition3D.SetZ(default);
                rects.fade.sizeDelta = savedSizeDelta;
            }
        }


        private void RefreshNextLeagueBackground_OnUpdate(float deltaTime) =>
            RefreshNextLeagueBackground(default);


        #endregion
    }
}
