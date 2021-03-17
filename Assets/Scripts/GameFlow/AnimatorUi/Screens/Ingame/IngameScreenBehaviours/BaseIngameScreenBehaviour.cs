using System;
using DG.Tweening;
using Drawmasters.Advertising;
using Drawmasters.Announcer;
using Drawmasters.Levels;
using Drawmasters.Levels.Data;
using Drawmasters.Levels.Helpers;
using Drawmasters.ServiceUtil;
using Drawmasters.ServiceUtil.Interfaces;
using Modules.Advertising;
using Modules.Analytics;
using Modules.General.Abstraction;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using I2.Loc;
using PetsData = Drawmasters.Ui.PetIngameScreenBehaviour.Data;


namespace Drawmasters.Ui
{
    public abstract class BaseIngameScreenBehaviour : IInitializable, 
        IDeinitializable
    {
        #region Helpers

        [Serializable]
        public class Data
        {
            [Required] public Localize levelNumberText = default;
            [Required] public Button pauseButton = default;
            [Required] public Button reloadButton = default;
            [Required] public StagesUiHandler stagesHandler = default;
            [Required] public PathProgressBar pathProgressBar = default;
            [Required] public RewardedVideoButton skipLevelButton = default;
            [Required] public Button skipLevelAnimationButton = default;

            [Required] public Transform outOfInkAnimatable = default;

            [Header("Temp hotfix")]
            [Required] public CanvasGroup reloadButtonCanvas = default;
            public FactorAnimation reloadButtonHideAnimation = default;

            public PetsData petsData = default;
        }

        #endregion



        #region Fields

        protected readonly IngameScreen screen;
        protected readonly Data data;
        protected readonly ICommonStatisticsService commonStatisticsService;
        
        private readonly PetIngameScreenBehaviour petBehaviour;
        private readonly AnnouncerHandler announcerHandler;

        private readonly object reloadAnimationToken;
        protected Animator skipLevelButtonAnimator;
       
        private bool isHitmastersLiveOps;


        #endregion



        #region Properties

        protected abstract Announcer.Announcer[] AvailableAnnouncers { get; }
        
        protected abstract bool IsSkipButtonAvailable { get; }

        public abstract bool IsCallPetButtonAvailable { get; }

        #endregion




        #region Ctor

        public BaseIngameScreenBehaviour(IngameScreen ingameScreen, Data _data)
        {
            screen = ingameScreen;
            data = _data;

            commonStatisticsService = GameServices.Instance.CommonStatisticService;
            
            announcerHandler = new AnnouncerHandler();
            reloadAnimationToken = Guid.NewGuid();

            petBehaviour = new PetIngameScreenBehaviour(_data.petsData);
        }
        
        #endregion
        
        
        
        #region IInitializable
        
        public virtual void Initialize()
        {
            int currentLevelIndex = commonStatisticsService.GetLevelsFinishedCount(GameServices.Instance.LevelEnvironment.Context.Mode);

            data.levelNumberText.SetStringParams(currentLevelIndex + 1);
            
            foreach (var availableAnnouncer in AvailableAnnouncers)
            {
                announcerHandler.AddAnnouncer(availableAnnouncer);
            }

            announcerHandler.Initialize();
            
            data.pathProgressBar.Initialize();
            data.stagesHandler.Initialize();
            
            CommonUtility.SetObjectActive(data.skipLevelButton.gameObject, IsSkipButtonAvailable);
            if (IsSkipButtonAvailable)
            {
                LevelContext context = GameServices.Instance.LevelEnvironment.Context;
                
                RuntimeAnimatorController skipButtonController =
                    IngameData.Settings.skipAnimationSettings.FindButtonAnimationController(context.LevelType);
                skipLevelButtonAnimator = data.skipLevelButton.GetComponent<Animator>();
                skipLevelButtonAnimator.runtimeAnimatorController = skipButtonController;

                data.skipLevelButton.Initialize(AdsVideoPlaceKeys.SkipLevel);
                data.skipLevelButton.OnVideoShowEnded += BonusButton_OnVideoShowEnded;
            }

            data.reloadButtonCanvas.alpha = 1.0f;
            Level.OnLevelStateChanged += Level_OnLevelStateChanged;

            LevelContext levelContext = GameServices.Instance.LevelEnvironment.Context;
            isHitmastersLiveOps = levelContext.Mode.IsHitmastersLiveOps();

            petBehaviour.Initialize(IsCallPetButtonAvailable, levelContext);

            petBehaviour.OnInvokePet += PetIngameScreenBehaviour_OnInvokePet;
        }

        #endregion


        
        #region IDeinitializable

        public virtual void Deinitialize()
        {
            DOTween.Kill(this);
            DOTween.Kill(reloadAnimationToken);

            announcerHandler.Deinitialize();
            data.pathProgressBar.Deinitialize();
            data.stagesHandler.Deinitialize();

            petBehaviour.Deinitialize();

            if (IsSkipButtonAvailable)
            {
                data.skipLevelButton.OnVideoShowEnded -= BonusButton_OnVideoShowEnded;
                data.skipLevelButton.Deinitialize();
            }

            Level.OnLevelStateChanged -= Level_OnLevelStateChanged;
        }

        #endregion



        #region Public methods

        public void InitializeButtons()
        {
            data.skipLevelButton.InitializeButtons();
            data.pauseButton.onClick.AddListener(PauseButton_OnClick);
            data.reloadButton.onClick.AddListener(ReloadButton_OnButtonClick);

            petBehaviour.InitializeButtons();
        }


        public void DeinitializeButtons()
        {
            data.skipLevelButton.DeinitializeButtons();
            data.pauseButton.onClick.RemoveListener(PauseButton_OnClick);
            data.reloadButton.onClick.RemoveListener(ReloadButton_OnButtonClick);

            petBehaviour.DeinitializeButtons();
        }

        #endregion



        #region Private methods

        protected virtual void OnLevelReloaded()
        {
            petBehaviour.OnLevelReload();

            bool canReload = LevelsManager.Instance.Level.CurrentState.CanReload();
            if (canReload)
            {
                LevelsManager.Instance.CompleteLevel(LevelResult.Reload);
            }
        }

        #endregion



        #region Events handlers

        private void PauseButton_OnClick()
        {
            TouchManager.Instance.IsEnabled = false;
            GameManager.Instance.SetGamePaused(true, this);

            LevelState savedLevelState = LevelsManager.Instance.Level.CurrentState;
            LevelsManager.Instance.Level.ChangeState(LevelState.Paused);

            UiScreenManager.Instance.ShowScreen(ScreenType.Result, null, (showedView) =>
            {
                TouchManager.Instance.IsEnabled = true;
                GameManager.Instance.SetGamePaused(false, this);

                LevelContext context = GameServices.Instance.LevelEnvironment.Context;
                if (!context.Mode.IsSceneMode() &&
                    !context.Mode.IsProposalSceneMode())
                {
                    LevelsManager.Instance.Level.ChangeState(savedLevelState);
                }
            });
        }


        private void ReloadButton_OnButtonClick()
        {
            DeinitializeButtons();

            AdvertisingManager.Instance.TryShowAdByModule(AdModule.Interstitial, 
                AdPlacementType.InGameRestart,
                result =>
                {
                    if (screen != null &&
                        !screen.IsNull())
                    {
                        InitializeButtons();

                        OnLevelReloaded();
                    }
                    else
                    {
                        CustomDebug.Log($"Interstitial was showed with delay. While delay level has been already finished");
                    }
                });
        }
        
        
        private void BonusButton_OnVideoShowEnded(AdActionResultType result)
        {
            if (result == AdActionResultType.Success)
            {
                bool canSkip = LevelsManager.Instance.Level.CurrentState.CanSkipOnIngame();
                if (canSkip)
                {
                    LevelsManager.Instance.CompleteLevel(LevelResult.IngameSkip);

                    CommonEvents.SendAdVideoReward(data.skipLevelButton.Placement);
                }
            }
        }


        private void Level_OnLevelStateChanged(LevelState state)
        {
            if (isHitmastersLiveOps)
            {
                bool isLoseState = state == LevelState.OutOfAmmo ||
                                   state == LevelState.EndPlaying ||
                                   state == LevelState.AllTargetsHitted;
                if (isLoseState &&
                    data.reloadButtonCanvas.alpha > 0.0f)
                {
                    data.reloadButtonHideAnimation.Play(e => data.reloadButtonCanvas.alpha = e, reloadAnimationToken);
                }

                // Hotfix. Vladislav.k. The case below is valid only for devices
                bool isInitialState = state == LevelState.Initialized ||
                                      state == LevelState.Playing;

                if (isInitialState)
                {
                    DOTween.Kill(reloadAnimationToken);
                    
                    data.reloadButtonCanvas.alpha = 1.0f;
                }
            }
        }

        
        private void PetIngameScreenBehaviour_OnInvokePet()
        {
            CommonUtility.SetObjectActive(data.pauseButton.gameObject, false);
            CommonUtility.SetObjectActive(data.reloadButton.gameObject, false);
            CommonUtility.SetObjectActive(data.skipLevelButton.gameObject, false);
        }

        #endregion
    }
}
