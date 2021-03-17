using UnityEngine;
using UnityEngine.UI;
using Drawmasters.ServiceUtil;
using System;
using System.Collections.Generic;
using Drawmasters.Levels;
using Drawmasters.Proposal;
using Modules.Advertising;
using Modules.General.Abstraction;
using TMPro;
using Drawmasters.Helpers;
using Modules.General;


namespace Drawmasters.Ui
{
    public abstract class UiSkinScreen : AnimatorScreen
    {
        #region Fields

        [SerializeField] private UiHudTopSelector uiHudTopSelector = default;
        [SerializeField] private Button[] settingsButtons = default;
        [SerializeField] private TMP_Text currentLevelText = default;
        [SerializeField] private Button[] menuButtons = default;
        [SerializeField] private CameraOffsetSettings cameraOffsetSettings = default;

        protected SkinScreenState scrollState;

        protected UISkinScrollBehaviour currentBehaviour;

        private Dictionary<SkinScreenState, UISkinScrollBehaviour> behaviours;
        private bool isHoldingIngameTouchMonitor;

        #endregion



        #region Properties

        protected abstract SkinScreenState InitialScrollState { get; }

        protected abstract Dictionary<SkinScreenState, UISkinScrollBehaviour> InitialBehaviours { get; }

        protected UISkinScrollBehaviour CurrentScrollBehaviour
        {
            get
            {
                UISkinScrollBehaviour behaviour = null;
                
                if (behaviours.TryGetValue(scrollState, out UISkinScrollBehaviour resultBehaviour))
                {
                    behaviour = resultBehaviour;
                }

                return behaviour;
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

            uiHudTopSelector.ShowActualUiHudTop();
            uiHudTopSelector.ActualUiHudTop.InitializeCurrencyRefresh();
            uiHudTopSelector.ActualUiHudTop.SetupExcludedTypes(CurrencyType.MansionHammers, CurrencyType.RollBones);
            uiHudTopSelector.ActualUiHudTop.RefreshCurrencyVisual(0f);

            GameMode mode = GameServices.Instance.PlayerStatisticService.PlayerData.LastPlayedMode;
            currentLevelText.text = mode.UiHeaderText();

            scrollState = InitialScrollState;
            behaviours = new Dictionary<SkinScreenState, UISkinScrollBehaviour>(InitialBehaviours);
        }


        public override void Deinitialize()
        {
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);

            uiHudTopSelector.ActualUiHudTop.DeinitializeCurrencyRefresh();

            currentBehaviour.Clear();
            currentBehaviour.Deinitialize();    

            base.Deinitialize();
        }


        public override void Show()
        {
            base.Show();

            IngameCamera.Instance.MoveLocalOffSetY(cameraOffsetSettings.offsetY, cameraOffsetSettings.animation.duration,
                cameraOffsetSettings.animation.curve);

            RefreshBehaviour();
        }


        public override void Hide()
        {
            DeinitializeButtons();

            base.Hide();
        }


        public override void InitializeButtons()
        {
            IngameTouchMonitor.OnAnyTap += IngameTouchMonitor_OnAnyTap;
            IngameTouchMonitor.OnAnyUp += IngameTouchMonitor_OnAnyUp;

            foreach (var settingsButton in settingsButtons)
            {
                settingsButton.onClick.AddListener(SettingsButton_OnClick);
            }

            foreach (var menuButton in menuButtons)
            {
                menuButton.onClick.AddListener(MenuButton_OnClicked);
            }
        }


        public override void DeinitializeButtons()
        {
            IngameTouchMonitor.OnAnyTap -= IngameTouchMonitor_OnAnyTap;
            IngameTouchMonitor.OnAnyUp -= IngameTouchMonitor_OnAnyUp;

            foreach (var settingsButton in settingsButtons)
            {
                settingsButton.onClick.RemoveListener(SettingsButton_OnClick);
            }

            foreach (var menuButton in menuButtons)
            {
                menuButton.onClick.RemoveListener(MenuButton_OnClicked);
            }
        }


        protected void RefreshBehaviour()
        {
            UISkinScrollBehaviour nextBehaviour = CurrentScrollBehaviour;

            if (currentBehaviour != null)
            {
                currentBehaviour.OnShouldReceiveReward -= OnReceiveReward;
                currentBehaviour.Disable();
            }
            
            currentBehaviour = nextBehaviour;
            
            if (currentBehaviour != null)
            {
                currentBehaviour.Enable();
                currentBehaviour.OnShouldReceiveReward += OnReceiveReward;
            }
        }


        protected string GetShowTabTriggerKey(SkinScreenState scrollState, SkinScreenState state) =>
            string.Concat(behaviours[scrollState].AnimatorTrigerKey, behaviours[state].AnimatorTrigerKey);

        #endregion



        #region Events handlers

        private void IngameTouchMonitor_OnAnyUp()
        {
            Scheduler.Instance.CallMethodWithDelay(this, () =>
            {
                isHoldingIngameTouchMonitor = false;
            }, 0.1f); // TODO: IDK why such long delay. TO vladislav k
        }

        private void IngameTouchMonitor_OnAnyTap()
        {
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
            isHoldingIngameTouchMonitor = true;

        }


        private void MenuButton_OnClicked()
        {
            if (isHoldingIngameTouchMonitor)
            {
                return;
            }

            DeinitializeButtons();

            Hide(view => UiScreenManager.Instance.ShowScreen(ScreenType.MainMenu,
                    menuScreen =>
                    {
                        AdvertisingManager.Instance.TryShowAdByModule(AdModule.Interstitial,
                            AdPlacementType.GalleryClose);
                    }), null);
        }


        private void SettingsButton_OnClick() =>
            UiScreenManager.Instance.ShowScreen(ScreenType.Settings, null, isForceHideIfExist: true);


        private void OnReceiveReward(RewardData rewardData)
        {
            UiScreenManager.Instance.ShowScreen(ScreenType.SpinReward, onShowBegin: (view) =>
            {
                if (view is UiRewardReceiveScreen rewardScreen)
                {
                    rewardScreen.SetupFxKey(EffectKeys.FxGUICharacterOpenShine);
                    rewardScreen.SetupReward(rewardData);
                }
            });
        }

        #endregion
    }
}