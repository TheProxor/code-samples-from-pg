using System;
using DG.Tweening;
using Drawmasters.Analytic;
using Modules.Sound;
using Drawmasters.Levels;
using Drawmasters.Announcer;
using Drawmasters.Proposal;
using Drawmasters.ServiceUtil;
using Drawmasters.Utils.Ui;
using Modules.General;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


namespace Drawmasters.Ui.Hitmasters
{
    public class UiHitmastersMapScreen : RewardReceiveScreen
    {
        #region Fields
        
        public static event Action OnShouldFinishLiveOps;

        [Header("Common")]
        [SerializeField] private Button backButton = default;
        [SerializeField] private HitmastersMap hitmastersMap = default;
        [SerializeField] private UiHitmastersMapPropose uiPropose = default;
        [SerializeField] private Button playButton = default;

        [Header("Scroll")]
        [SerializeField] private RubberScrollRect scrollRect = default;
        [SerializeField] private EventTrigger scrollRectEventTrigger = default;
       

        [Header("Announcers")]
        [SerializeField] private Transform liveOpsEndAnimatable = default;

        private HitmastersProposeController controller;

        private readonly AnnouncerHandler announcerHandler = new AnnouncerHandler();

        private RubberScrollHelper rubberScrollHelper;
        private int currentScrolledPoint = -1;

        #endregion



        #region Properties

        public override ScreenType ScreenType =>
            ScreenType.HitmastersMap;

        #endregion



        #region Methods

        public void MarkMenuEnter()
        {
            if (TryFindPointIndexToScroll(out int index, true))
            {
                ScrollToPoint(index);
            }
        }


        public void PlayLevelCompleteAnimation(int completedLevel) =>
            hitmastersMap.PlayLevelCompleteAnimation(completedLevel);


        public override void Initialize(Action<AnimatorView> onShowEndCallback = null,
            Action<AnimatorView> onHideEndCallback = null,
            Action<AnimatorView> onShowBeginCallback = null,
            Action<AnimatorView> onHideBeginCallback = null)
        {
            base.Initialize(onShowEndCallback, onHideEndCallback, onShowBeginCallback, onHideBeginCallback);

            controller = GameServices.Instance.ProposalService.HitmastersProposeController;
            
            uiHudTop.InitializeCurrencyRefresh(true);
            uiHudTop.SetupExcludedTypes(CurrencyType.MansionHammers, CurrencyType.RollBones);
            uiHudTop.RefreshCurrencyVisual(0.0f);
            
            hitmastersMap.Initialize();
            uiPropose.Initialize();

            controller.OnFinished += BackButton_OnClick;

            announcerHandler.AddAnnouncer(new HitmastersLiveOpsCompleteAnnouncer(this, liveOpsEndAnimatable));
            announcerHandler.Initialize();

            SoundManager.Instance.PlaySound(AudioKeys.Music.DRAWMASTERS_LIVEOPS_MAP_THEME, isLooping: true);
        }


        public override void Deinitialize()
        {
            uiHudTop.DeinitializeCurrencyRefresh();
            uiPropose.Deinitialize();
            hitmastersMap.Deinitialize();
            announcerHandler.Deinitialize();

            controller.OnFinished -= BackButton_OnClick;

            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
            DOTween.Kill(rubberScrollHelper);

            rubberScrollHelper.RemoveCallback(OnScrollBegin, EventTriggerType.BeginDrag);
            rubberScrollHelper = null;

            base.Deinitialize();
        }

        
        public override void InitializeButtons()
        {
            backButton.onClick.AddListener(BackButton_OnClick);
            playButton.onClick.AddListener(PlayButton_OnClick);
        }

        
        public override void DeinitializeButtons()
        {
            backButton.onClick.RemoveListener(BackButton_OnClick);
            playButton.onClick.RemoveListener(PlayButton_OnClick);
        }


        public override void Show()
        {
            base.Show();

            // Create on show to skip one frame for internal UI calculations
            rubberScrollHelper = new RubberScrollHelper(scrollRect, mainCanvas, scrollRectEventTrigger);
            rubberScrollHelper.AddCallback(OnScrollBegin, EventTriggerType.BeginDrag);

            CommonUtility.SetObjectActive(playButton.gameObject, !controller.IsCurrentLiveOpsCompleted);

            if (controller.IsCurrentLiveOpsCompleted)
            {
                DeinitializeButtons();

                controller.IsCurrentLiveOpsTaskCompleted = true;
                AnalyticHelper.SendLiveOpsCompleteEvent(controller.LiveOpsAnalyticName, controller.LiveOpsAnalyticEventId);
                    
                Scheduler.Instance.CallMethodWithDelay(this, () =>
                OnShouldApplyReward(controller.GeneratedLiveOpsReward, () => OnShouldFinishLiveOps?.Invoke()),
                    controller.VisualSettings.rewardPopupTimeout);
                
            }

            if (controller.ShouldScrollMapOnShow)
            {
                ScrollAllMap();
                controller.MarkMapOnShowScrolled();
            }
            else
            {
                MarkMenuEnter();
            }
        }


        private void ScrollAllMap()
        {
            rubberScrollHelper.Scroll(rubberScrollHelper.ScrollTopPosition,
                                      rubberScrollHelper.ScrollBottomPosition,
                                      controller.VisualSettings.swipeAllMapAnimation,
                                      rubberScrollHelper,
                                      MarkMenuEnter);
        }


        private bool TryFindPointIndexToScroll(out int pointIndexForScroll, bool isStart = false)
        {
            pointIndexForScroll = hitmastersMap.CurrentPointIndex;
            
            return isStart || currentScrolledPoint != pointIndexForScroll;
        }


        private void ScrollToPoint(int indexToScroll)
        {
            // HACK
            indexToScroll = Mathf.Clamp(indexToScroll, 0, hitmastersMap.PointsLength - 1);
            int prewIndexToScroll = Mathf.Clamp(indexToScroll - 1, 0, hitmastersMap.PointsLength - 1);

            OnScrollBegin(default);

            RectTransform prewTargetRectTransform = hitmastersMap.GetPointRectTransform(prewIndexToScroll);
            Vector3 prewEndScrollPosition = rubberScrollHelper.GetCentredSnapPosition(prewTargetRectTransform);
            
            RectTransform targetRectTransform = hitmastersMap.GetPointRectTransform(indexToScroll);
            Vector3 endScrollPosition = rubberScrollHelper.GetCentredSnapPosition(targetRectTransform);

            if (endScrollPosition.y < rubberScrollHelper.ScrollBottomPosition.y &&
                endScrollPosition.y > rubberScrollHelper.ScrollTopPosition.y)
            {
                rubberScrollHelper.Scroll(prewEndScrollPosition,
                                          endScrollPosition,
                                          controller.VisualSettings.swipePointsAnimation,
                                          rubberScrollHelper);
            }
            else if (endScrollPosition.y < rubberScrollHelper.ScrollTopPosition.y)
            {
                rubberScrollHelper.Scroll(rubberScrollHelper.ScrollTopPosition,
                    rubberScrollHelper.ScrollTopPosition,
                    controller.VisualSettings.swipePointsAnimation,
                    rubberScrollHelper);
            }

            currentScrolledPoint = indexToScroll;
        }


        public override Vector3 GetCurrencyStartPosition(RewardData rewardData) =>
            default;
        
        #endregion



        #region Events handlers
        
        private void BackButton_OnClick()
        {
            DeinitializeButtons();

            FromLevelToLevel.PlayTransition(() =>
            {
                HideImmediately();

                LevelsManager.Instance.LoadScene(GameServices.Instance.PlayerStatisticService.PlayerData.LastPlayedMode, GameMode.MenuScene);
                UiScreenManager.Instance.ShowScreen(ScreenType.MainMenu, isForceHideIfExist: true);

                GameServices.Instance.MusicService.InstantRefreshMusic();
            });
        }


        private void PlayButton_OnClick() =>
            GameServices.Instance.ProposalService.HitmastersProposeController.Propose();


        private void OnScrollBegin(BaseEventData baseEventData) =>
            SoundManager.Instance.PlaySound(AudioKeys.Ui.LIVEOPS_MAP_SCROLL);

        #endregion
    }
}
