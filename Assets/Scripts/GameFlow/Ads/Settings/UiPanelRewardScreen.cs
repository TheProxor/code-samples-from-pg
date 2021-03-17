using System;
using Modules.General.Abstraction;
using Drawmasters.Proposal;
using Drawmasters.Proposal.Interfaces;
using Drawmasters.ServiceUtil;
using Modules.Advertising;
using Modules.Analytics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Drawmasters.Ui
{
    public abstract class UiPanelRewardScreen : AnimatorScreen
    {
        #region Fields

        private const float updateFrameRate = 0.5f;

        [SerializeField] private GameObject rewardRoot = default;
        [SerializeField] private RewardedVideoButton rewardButton = default;
        [SerializeField] private Button disabledRewardButton = default;
        [SerializeField] private TMP_Text timeLeftRewardText = default;
        [SerializeField] private TMP_Text currentLevelText = default;
        [SerializeField] private Button[] menuButtons = default;

        private float updateDelay;

        #endregion



        #region Properties

        protected abstract UiPanelRewardController Controller { get; }

        protected abstract IProposable Proposal { get; }

        protected abstract string VideoPlacementKey { get; }

        #endregion



        #region Methods

        public override void Initialize(Action<AnimatorView> onShowEndCallback = null,
                                        Action<AnimatorView> onHideEndCallback = null,
                                        Action<AnimatorView> onShowBeginCallback = null,
                                        Action<AnimatorView> onHideBeginCallback = null)
        {
            base.Initialize(onShowEndCallback, 
                            onHideEndCallback, 
                            onShowBeginCallback, 
                            onHideBeginCallback);

            rewardButton.Initialize(VideoPlacementKey);

            rewardButton.OnVideoShowEnded += OnReceiveReward;

            GameMode mode = GameServices.Instance.PlayerStatisticService.PlayerData.LastPlayedMode;

            currentLevelText.text = mode.UiHeaderText();

            updateDelay = 0f;

            RefreshProposalView();
        }


        public override void Deinitialize()
        {
            rewardButton.Deinitialize();

            rewardButton.OnVideoShowEnded -= OnReceiveReward;

            base.Deinitialize();
        }

        public override void Show()
        {
            base.Show();

            MonoBehaviourLifecycle.OnUpdate += MonoBehaviourLifecycle_OnUpdate;
        }


        public override void Hide()
        {
            MonoBehaviourLifecycle.OnUpdate -= MonoBehaviourLifecycle_OnUpdate;

            base.Hide();
        }


        public override void InitializeButtons()
        {
            rewardButton.InitializeButtons();

            foreach (var menuButton in menuButtons)
            {
                menuButton.onClick.AddListener(MenuButton_OnClicked);
            }
        }


        public override void DeinitializeButtons()
        {
            rewardButton.DeinitializeButtons();

            foreach (var menuButton in menuButtons)
            {
                menuButton.onClick.RemoveListener(MenuButton_OnClicked);
            }
        }


        private void RefreshProposalView()
        {
            if (this.IsNull())
            {
                return;
            }

            timeLeftRewardText.text = Controller.Settings.UiTimeLeft;

            CommonUtility.SetObjectActive(rewardRoot, Proposal.IsAvailable);
            CommonUtility.SetObjectActive(rewardButton.gameObject, Proposal.CanPropose);
            CommonUtility.SetObjectActive(disabledRewardButton.gameObject, !Proposal.CanPropose);
        }

        #endregion



        #region Events handlers

        private void MenuButton_OnClicked()
        {
            DeinitializeButtons();
            
            Hide(view => UiScreenManager.Instance.ShowScreen(ScreenType.MainMenu, menuScreen =>
            {
                AdvertisingManager.Instance.TryShowAdByModule(AdModule.Interstitial, AdPlacementType.GalleryClose);
            }), null);
        }

        private void MonoBehaviourLifecycle_OnUpdate(float deltaTime)
        {
            updateDelay += deltaTime;

            if (updateDelay > updateFrameRate)
            {
                RefreshProposalView();

                updateDelay = 0f;
            }
        }


        private void OnReceiveReward(AdActionResultType result)
        {
            if (result == AdActionResultType.Success)
            {
                OnShouldReceiveReward();
            }
        }


        private void OnShouldReceiveReward()
        {
            //HACK
            {
                Proposal.Propose(null);
            }
            
            RewardData rewardData = Controller.Settings.GetRandomReward(Controller.ShowsCount);
            rewardData.Open();
            
            CommonEvents.SendAdVideoReward(VideoPlacementKey);

            UiScreenManager.Instance.ShowScreen(ScreenType.SpinReward, onShowBegin: (view) =>
            {
                if (view is UiRewardReceiveScreen rewardScreen)
                {
                    rewardScreen.SetupFxKey(EffectKeys.FxGUICharacterOpenShine);
                    rewardScreen.SetupReward(rewardData);
                }
            });

            Controller.Settings.MarkVideoWatched();
            OnRewardReceived();
        }


        protected virtual void OnRewardReceived() { }

        #endregion
    }
}
