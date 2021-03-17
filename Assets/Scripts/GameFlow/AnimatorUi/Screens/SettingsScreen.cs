using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Drawmasters.Vibration;
using Modules.Advertising;
using Modules.General.Abstraction;
using Modules.General.InAppPurchase;
using Modules.Sound;
using Drawmasters.Statistics.Data;
using Drawmasters.ServiceUtil;
using Drawmasters.Utils.Ui;
using Modules.Networking;
using Modules.Notification;


namespace Drawmasters.Ui
{
    public class SettingsScreen : AnimatorScreen
    {
        #region Fields

        [SerializeField] private Button hideButton = default;

        [SerializeField] private UserInputField userNickNameInputField  = default;
        [SerializeField] private SettingsScreenButton musicButton = default;
        [SerializeField] private SettingsScreenButton soundButton = default;
        [SerializeField] private SettingsScreenButton vibrationButton = default;
        [SerializeField] private SettingsScreenButton notificationsButton = default;
        [SerializeField] private SettingsScreenButton bloodButton = default;

        [SerializeField] private Button restorePurchasesButton = default;

        [SerializeField] private Button termOfUseButton = default;

        [SerializeField] private NoAdsPropose noAdsPropose = default;

        private List<SettingsScreenButton> buttons;

        #endregion



        #region Properties

        public override ScreenType ScreenType => ScreenType.Settings;

        #endregion



        #region Methods

        public override void Initialize(Action<AnimatorView> onShowEndCallback = null,
                                        Action<AnimatorView> onHideEndCallback = null,
                                        Action<AnimatorView> onShowBegin = null,
                                        Action<AnimatorView> onHideBegin = null)
        {
            base.Initialize(onShowEndCallback, onHideEndCallback, onShowBegin, onHideBegin);

            buttons = new List<SettingsScreenButton>
            {
                vibrationButton,
                musicButton,
                soundButton,
                notificationsButton,
                bloodButton
            };

            foreach (var button in buttons)
            {
                button.Initialize();
            }

            RefreshButtonsState();

            CommonUtility.SetObjectActive(termOfUseButton.gameObject, LLPrivacyManager.Instance.IsPrivacyButtonAvailable);

            bool isRestoreButtonAvailable = GameServices.Instance.CommonStatisticService.IsIapsAvailable;
#if UNITY_IOS
            CommonUtility.SetObjectActive(restorePurchasesButton.gameObject, isRestoreButtonAvailable);
#elif UNITY_ANDROID
            CommonUtility.SetObjectActive(restorePurchasesButton.gameObject, false);
#endif
            noAdsPropose.Initialize();

            bool shouldShowUserNickNameInputField = GameServices.Instance.ProposalService.LeagueProposeController.WasFirstLiveOpsStarted &&
                                                    GameServices.Instance.ProposalService.LeagueProposeController.IsMechanicAvailable;
            userNickNameInputField.SetRootEnabled(shouldShowUserNickNameInputField);

            string userNickName = GameServices.Instance.ProposalService.LeagueProposeController.LeaderBoard.PlayerData.NickName;
            userNickNameInputField.SetText(userNickName);

            userNickNameInputField.Initialize();
            userNickNameInputField.OnInputSubmited += UserNickNameInputField_OnInputSubmited;
        }


        public override void Deinitialize()
        {
            GameManager.Instance.MayDataBeDeleted = false;

            foreach (var button in buttons)
            {
                button.Deinitialize();
            }

            noAdsPropose.Deinitialize();
            userNickNameInputField.Deinitialize();
            userNickNameInputField.OnInputSubmited -= UserNickNameInputField_OnInputSubmited;

            base.Deinitialize();
        }


        public override void InitializeButtons()
        {
            termOfUseButton.onClick.AddListener(TermOfUseButton_OnClick);

            hideButton.onClick.AddListener(HideButton_OnClicked);

            vibrationButton.AddButtonOnClickCallback(EnableVibration, false);
            vibrationButton.AddButtonOnClickCallback(DisableVibration, true);

            musicButton.AddButtonOnClickCallback(EnableMusic, false);
            musicButton.AddButtonOnClickCallback(DisableMusic, true);

            soundButton.AddButtonOnClickCallback(EnableSound, false);
            soundButton.AddButtonOnClickCallback(DisablSound, true);

            notificationsButton.AddButtonOnClickCallback(EnableNotifications, false);
            notificationsButton.AddButtonOnClickCallback(DisableNotifications, true);

            bloodButton.AddButtonOnClickCallback(EnableBlood, false);
            bloodButton.AddButtonOnClickCallback(DisableBlood, true);

            restorePurchasesButton.onClick.AddListener(RestorePurchasesButton_OnClick);
        }


        public override void DeinitializeButtons()
        {
            hideButton.onClick.RemoveListener(HideButton_OnClicked);

            vibrationButton.RemoveButtonOnClickCallback(EnableVibration, false);
            vibrationButton.RemoveButtonOnClickCallback(DisableVibration, true);

            musicButton.RemoveButtonOnClickCallback(EnableMusic, false);
            musicButton.RemoveButtonOnClickCallback(DisableMusic, true);

            soundButton.RemoveButtonOnClickCallback(EnableSound, false);
            soundButton.RemoveButtonOnClickCallback(DisablSound, true);

            notificationsButton.RemoveButtonOnClickCallback(EnableNotifications, false);
            notificationsButton.RemoveButtonOnClickCallback(DisableNotifications, true);

            bloodButton.RemoveButtonOnClickCallback(EnableBlood, false);
            bloodButton.RemoveButtonOnClickCallback(DisableBlood, true);

            restorePurchasesButton.onClick.RemoveListener(RestorePurchasesButton_OnClick);

            termOfUseButton.onClick.RemoveListener(TermOfUseButton_OnClick);
        }


        private void RefreshButtonsState()
        {
            PlayerData playerData = GameServices.Instance.PlayerStatisticService.PlayerData;

            vibrationButton.SetEnabled(VibrationManager.IsVibrationEnabled);

            bool isMusicMuted = SoundManager.Instance.IsMutedCategory(MusicCategory.Music);
            musicButton.SetEnabled(!isMusicMuted);

            bool isSoundsMuted = SoundManager.Instance.IsMutedCategory(MusicCategory.Ingame);
            soundButton.SetEnabled(!isSoundsMuted);

            notificationsButton.SetEnabled(NotificationManager.Instance.AreNotificationsEnabled);
            bloodButton.SetEnabled(playerData.IsBloodEnabled);
        }

        #endregion



        #region Events handlers

        private void HideButton_OnClicked()
        {
            Hide(view =>
            {
                AdvertisingManager.Instance.TryShowAdByModule(AdModule.Interstitial, AdPlacementType.SettingsClose);
            }, null);
        }

        private void EnableVibration()
        {
            VibrationManager.IsVibrationEnabled = true;
            RefreshButtonsState();
        }


        private void DisableVibration()
        {
            VibrationManager.IsVibrationEnabled = false;
            RefreshButtonsState();
        }


        private void EnableMusic()
        {
            SoundManager.Instance.MuteCategory(MusicCategory.Music, false);
            SoundManager.Instance.SaveMutedCategoriesData();

            RefreshButtonsState();
        }


        private void DisableMusic()
        {
            SoundManager.Instance.MuteCategory(MusicCategory.Music, true);
            SoundManager.Instance.SaveMutedCategoriesData();

            RefreshButtonsState();
        }


        private void EnableSound()
        {
            SoundManager.Instance.MuteCategory(MusicCategory.Ingame, false);
            SoundManager.Instance.MuteCategory(MusicCategory.Ui, false);
            SoundManager.Instance.SaveMutedCategoriesData();

            RefreshButtonsState();
        }


        private void DisablSound()
        {
            SoundManager.Instance.MuteCategory(MusicCategory.Ingame, true);
            SoundManager.Instance.MuteCategory(MusicCategory.Ui, true);
            SoundManager.Instance.SaveMutedCategoriesData();

            RefreshButtonsState();
        }


        private void EnableNotifications()
        {
            GameServices.Instance.NotificationService.MarkQueryShowed();
            NotificationManager.Instance.AreNotificationsEnabled = true;
            RefreshButtonsState();
        }


        private void DisableNotifications()
        {
            NotificationManager.Instance.AreNotificationsEnabled = false;
            RefreshButtonsState();
        }


        private void EnableBlood()
        {
            PlayerData playerData = GameServices.Instance.PlayerStatisticService.PlayerData;

            playerData.IsBloodEnabled = true;
            RefreshButtonsState();
        }


        private void DisableBlood()
        {
            PlayerData playerData = GameServices.Instance.PlayerStatisticService.PlayerData;

            playerData.IsBloodEnabled = false;
            RefreshButtonsState();
        }


        private void TermOfUseButton_OnClick()
        {
            if (ReachabilityHandler.Instance.NetworkStatus == NetworkStatus.NotReachable)
            {
                UiScreenManager.Instance.ShowScreen(ScreenType.NoInternet);
                return;
            }
            
            LLPrivacyManager.Instance.GetTermsAndPolicyURI(((bool success, string url) =>
            {
                if (success)
                {
                    GameManager.Instance.MayDataBeDeleted = true;
                    
                    Application.OpenURL(url);
                }
                else
                {
                    UiScreenManager.Instance.ShowPopup(OkPopupType.SomethingWentWrong);
                }
            }));
        }


        private void RestorePurchasesButton_OnClick()
        {
            if (ReachabilityHandler.Instance.NetworkStatus == NetworkStatus.NotReachable)
            {
                UiScreenManager.Instance.ShowScreen(ScreenType.NoInternet);
                return;
            }

            UiScreenManager.Instance.ShowScreen(ScreenType.LoadingScreen);

            StoreManager.Instance.RestorePurchases((result) =>
            {
                UiScreenManager.Instance.HideScreen(ScreenType.LoadingScreen, (view) =>
                {
                    OkPopupType typeToShow = result.IsSucceed ? OkPopupType.RestoreOk : OkPopupType.RestoreFail;
                    UiScreenManager.Instance.ShowPopup(typeToShow);
                });
            });
        }


        private void UserNickNameInputField_OnInputSubmited(string text) =>
            GameServices.Instance.ProposalService.LeagueProposeController.LeaderBoard.PlayerData.NickName = text;
        
        #endregion
    }
}
