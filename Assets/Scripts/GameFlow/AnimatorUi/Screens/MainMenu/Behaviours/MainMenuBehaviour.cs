using System;
using System.Collections.Generic;
using Drawmasters.Interfaces;
using Drawmasters.Levels;
using Drawmasters.Levels.Data;
using Drawmasters.Proposal;
using Drawmasters.ServiceUtil;
using Drawmasters.ServiceUtil.Interfaces;
using Drawmasters.Ui.Enums;
using Drawmasters.Vibration;
using Modules.Advertising;
using Modules.General;
using Modules.General.Abstraction;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;


namespace Drawmasters.Ui
{
    public abstract class MainMenuBehaviour : IMainMenuBehaviour
    {
        #region Nested types

        [Serializable]
        public class BaseData
        {
            public GameObject rootObject = default;

            [Header("Live Opses and Proposals")]
            public UiProposal[] uiProposals = default;
            

            [Header("Bottom buttons")]
            [FormerlySerializedAs("uiShooterSkinPropose")] 
            public UiSkinPropose proposeIndicator = default;
            public UiSkinPropose uiWeaponSkinPropose = default;
            public CameraOffsetSettings cameraOffsetSettings = default;

            [Header("Common buttons")]
            public Button playButton = default;
            public Button[] shopMenuButtons = default;
            public Button[] settingsButtons = default;
        }

        #endregion



        #region Fields

        protected readonly List<GameObject> objects;

        protected readonly UiMainMenuScreen mainMenuScreen;

        protected UiLiveOpsProposeMonitor uiLiveOpsProposeMonitor;

        protected IPlayerStatisticService playerStatistics;

        private readonly BaseData baseData;
        private bool isBehaviourInitialized;

        #endregion


        #region Properties

        public CameraOffsetSettings CameraOffsetSettings => baseData.cameraOffsetSettings;

        public bool IsMechanicAvailable =>
            uaAbTestMechanic.WasAvailabilityChanged
                ? uaAbTestMechanic.IsMechanicAvailable
                : GameServices.Instance.AbTestService.CommonData.mainMenuScreenState == ScreenState;

        public abstract MainMenuScreenState ScreenState { get; }


        protected abstract IUaAbTestMechanic uaAbTestMechanic { get; }

        public bool IsAnyForceProposeActive
        {
            get => uiLiveOpsProposeMonitor.IsAnyForceProposeActive;
            set => uiLiveOpsProposeMonitor.IsAnyForceProposeActive = value;
        }

        #endregion


        #region Ctor

        protected MainMenuBehaviour(BaseData _baseData, UiMainMenuScreen screen)
        {
            baseData = _baseData;
            mainMenuScreen = screen;
            playerStatistics = GameServices.Instance.PlayerStatisticService;
            objects = new List<GameObject>();

            uiLiveOpsProposeMonitor = new UiLiveOpsProposeMonitor(baseData.uiProposals);
        }

        #endregion


        #region IResultBehaviour

        public virtual void Deinitialize()
        {
            Disable();
        }


        public virtual void Enable()
        {
            objects.ForEach(go => CommonUtility.SetObjectActive(go, true));

            uiLiveOpsProposeMonitor.Initialize();

            UiLiveOpsPropose.OnShouldForceProposeLiveOpsEvent += OnShouldForceProposeLiveOpsEvent;
            uiLiveOpsProposeMonitor.OnShouldForcePropose += AttemptMakeAnyForcePropose;

            VisitAllUiLiveOpsPropose((uiProposal) => uiProposal.Initialize());

            Scheduler.Instance.CallMethodWithDelay(this, AttemptMakeAnyForcePropose, CommonUtility.OneFrameDelay);

            GameMode currentGameMode = playerStatistics.PlayerData.LastPlayedMode;
            if (GameModesInfo.TryConvertModeToWeapon(currentGameMode, out WeaponType weaponType))
            {
                baseData.uiWeaponSkinPropose.SetupAvailableWeaponType(weaponType);
            }

            isBehaviourInitialized = true;
        }


        public virtual void Disable()
        {
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
            UiLiveOpsPropose.OnShouldForceProposeLiveOpsEvent -= OnShouldForceProposeLiveOpsEvent;

            if (uiLiveOpsProposeMonitor != null)
            {
                uiLiveOpsProposeMonitor.OnShouldForcePropose -= AttemptMakeAnyForcePropose;
                uiLiveOpsProposeMonitor.Deinitialize();
            }

            if (isBehaviourInitialized)
            {
                VisitAllUiLiveOpsPropose((uiProposal) => uiProposal.Deinitialize());
            }

            objects.ForEach(go => CommonUtility.SetObjectActive(go, false));

            isBehaviourInitialized = false;
        }


        public virtual void InitializeButtons()
        {
            foreach (var settingsButton in baseData.settingsButtons)
            {
                settingsButton.onClick.AddListener(SettingsButton_OnClick);
            }

            foreach (var shopMenuButton in baseData.shopMenuButtons)
            {
                shopMenuButton.onClick.AddListener(ShopMenuButton_OnClick);
            }

            baseData.playButton.onClick.AddListener(PlayButton_OnClicked);

            VisitAllUiLiveOpsPropose(e => e.InitializeButtons());
        }


        public virtual void DeinitializeButtons()
        {
            foreach (var settingsButton in baseData.settingsButtons)
            {
                settingsButton.onClick.RemoveListener(SettingsButton_OnClick);
            }

            foreach (var shopMenuButton in baseData.shopMenuButtons)
            {
                shopMenuButton.onClick.RemoveListener(ShopMenuButton_OnClick);
            }

            baseData.playButton.onClick.RemoveListener(PlayButton_OnClicked);

            VisitAllUiLiveOpsPropose(e => e.DeinitializeButtons());
        }

        #endregion



        #region Methods

        private void AttemptMakeAnyForcePropose()
        {
            Action proposeCallback = GetProposeCallback();

            if (proposeCallback != null)
            {
                IsAnyForceProposeActive = true;
            }

            proposeCallback?.Invoke();
        }


        private Action GetProposeCallback()
        {
            if (IsAnyForceProposeActive)
            {
                return default;
            }

            Action result = default;

            UiProposal uiLiveOpsProposeToPropose = Array.Find(baseData.uiProposals, e => e.CanForcePropose);
            UiProposal uiLiveOpsProposeEventToPropose = Array.Find(baseData.uiProposals, e => e.CanForceProposeLiveOpsEvent);
            if (uiLiveOpsProposeToPropose != null)
            {
                result = () => uiLiveOpsProposeToPropose.ForceProposeWithMenuShow();
            }
            else if (uiLiveOpsProposeEventToPropose != null)
            {
                result = () => uiLiveOpsProposeEventToPropose.ForceProposeEventWithMenuShow();
            }

            return result;
        }


        private void VisitAllUiLiveOpsPropose(Action<IUiProposal> callback)
        {
            if (callback == null)
            {
                return;
            }
            
            foreach (var propose in baseData.uiProposals)
            {
                callback.Invoke(propose);
            }
        }


        private void LoadLevel(GameMode mode)
        {
            LevelContext context = GameServices.Instance.LevelEnvironment.Context;

            if (context.Mode == mode)
            {
                DeinitializeButtons();

                FromLevelToLevel.PlayTransition(() =>
                {
                    mainMenuScreen.HideImmediately();

                    int currentLevelIndex = mode.GetCurrentLevelIndex();

                    LevelsManager.Instance.UnloadLevel();
                    LevelsManager.Instance.LoadLevel(mode, currentLevelIndex);

                    LevelsManager.Instance.PlayLevel();
                });

                VibrationManager.Play(IngameVibrationType.ModeEnterFromMainMenu);
            }
            else
            {
                LevelsManager.Instance.UnloadLevel();
                LevelsManager.Instance.LoadScene(mode, GameMode.MenuScene);
            }
        }

        #endregion


        #region Events hanlers

        private void PlayButton_OnClicked() =>
            LoadLevel(playerStatistics.PlayerData.LastPlayedMode);


        private void ShopMenuButton_OnClick() =>
            UiScreenManager.Instance.ShowScreen(ScreenType.ShopMenu);


        private void SettingsButton_OnClick()
        {
            DeinitializeButtons();

            AdvertisingManager.Instance.TryShowAdByModule(AdModule.Interstitial, AdPlacementType.SettingsOpen, result =>
            {
                InitializeButtons();

                Scheduler.Instance.CallMethodWithDelay(this,
                    () => UiScreenManager.Instance.ShowScreen(ScreenType.Settings), CommonUtility.OneFrameDelay);
            });
        }

        private void OnShouldForceProposeLiveOpsEvent(UiLiveOpsPropose uiLiveOpsProposal)
        {
            if (IsAnyForceProposeActive)
            {
                return;
            }

            uiLiveOpsProposal.ForceProposeEventWithMenuShow();
            IsAnyForceProposeActive = true;
        }

        #endregion
    }
}