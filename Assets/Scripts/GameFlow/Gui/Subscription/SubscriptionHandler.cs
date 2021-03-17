using System;
using UnityEngine;
using Drawmasters;
using Drawmasters.Helpers;
using Drawmasters.Ui;
using Drawmasters.ServiceUtil;
using Drawmasters.Statistics.Data;
using Modules.General;
using Modules.General.InAppPurchase;
using Modules.UiKit;


namespace Modules.InAppPurchase
{
    public class SubscriptionHandler
    {
        #region Public methods

        public void HandleStartSubscrption(Action onSubscriptionHidden = null)
        {
            if (SubscriptionManager.Instance.IsSubscriptionActive)
            {
                GiveRewardToUser(UiPopupType.DailyReward);
                
                onSubscriptionHidden?.Invoke();
            }
            else
            {
                bool shouldShowStartSubscription = !GameServices.Instance.CommonStatisticService.WasStartSubscrptionShowed;
                if (shouldShowStartSubscription)
                {
                    ShowSubscription(null, onSubscriptionHidden, true);
                }
                else
                {
                    onSubscriptionHidden?.Invoke();
                }
            }
        }


        public void HandleFromMenuSubscrption(Action onSubscriptionHidden = default) =>
            ShowSubscription(screen => screen.ForceMoveToLastPage(), onSubscriptionHidden);
        
        #endregion



        #region Private methods

        private void ShowSubscription(Action<UiDrawmastersSubscription4Screen> initializeCallback = default, 
                                      Action onSubscriptionHidden = default,
                                      bool isStartSubscription = false)
        {
            bool shouldShowStartSubscription = GameServices.Instance.AbTestService.CommonData.isSubscriptionAvailable &&
                                               GameServices.Instance.CommonStatisticService.IsIapsAvailable &&
                                               !StoreManager.Instance.HasAnyActiveSubscription;

            if (shouldShowStartSubscription)
            {
                SetUiKitCameraEnabled(true);

                IUiManager uiManager = Services.GetService<IUiManager>();

                IPopupSubscriptionSettings popupSettings = new PopupSubscriptionSettings()
                {
                    ResultCallback = result =>
                    {
                        SetUiKitCameraEnabled(false);
                        
                        switch (result)
                        {
                            case SubscriptionPopupResult.SubscriptionPurchased:
                            case SubscriptionPopupResult.SubscriptionRestored:
                                ApplySubscriptionReward();
                                GiveRewardToUser(UiKitPopupType.SubscriptionPurchasedReward, true);
                                break;

                            case SubscriptionPopupResult.SubscriptionClosed:
                            case SubscriptionPopupResult.None:
                            default:
                                break;
                        }

                        GameServices.Instance.CommonStatisticService.MarkStartSubscrptionShowed();
                        
                        onSubscriptionHidden?.Invoke();
                    }
                };

                UiDrawmastersSubscription4Screen shownPopup = 
                    uiManager.PopupManager.Show<UiDrawmastersSubscription4Screen>(UiPopupType.Subscription4Screen, popupSettings);

                initializeCallback?.Invoke(shownPopup);
            }
            else
            {
                onSubscriptionHidden?.Invoke();
            }
        }


        private void GiveRewardToUser(string uiPopupType, bool isForced = false)
        {
            if (SubscriptionManager.Instance.IsRewardPopupAvailable || isForced)
            {
                SetUiKitCameraEnabled(true);
                
                IUiManager uiManager = Services.GetService<IUiManager>();

                float simpleReward = IngameData.Settings.subscriptionRewardSettings.CurrencyRewardPerDay;
                float premiumReward = IngameData.Settings.subscriptionRewardSettings.PremiumCurrencyRewardPerDay;

                IDrawmastersSubscriptionRewardSettings popupSettings = new DrawmastersSubscriptionRewardSettings(
                    string.Empty, 
                    () =>
                    {
                        uiManager.PopupManager.HideCurrent();

                        GiveDailySubscriptionReward();
                    }, 
                    simpleReward.ToShortFormat(),
                    premiumReward.ToShortFormat());

                var shownPopup = uiManager.PopupManager.Show<DrawmastersSubscriptionRewardPopup>(uiPopupType, popupSettings);

                shownPopup.AddHiddenCallback(() => SetUiKitCameraEnabled(false));
            }
        }


        private void GiveDailySubscriptionReward()
        {
            float reward = IngameData.Settings.subscriptionRewardSettings.CurrencyRewardPerDay;
            GameServices.Instance.PlayerStatisticService.CurrencyData.AddCurrency(CurrencyType.Simple, reward);

            float premiumReward = IngameData.Settings.subscriptionRewardSettings.PremiumCurrencyRewardPerDay;
            GameServices.Instance.PlayerStatisticService.CurrencyData.AddCurrency(CurrencyType.Premium, premiumReward);
        }


        private void ApplySubscriptionReward()
        {
            //TODO why don't apply rewards??
            
            IngameData.Settings.subscriptionRewardSettings.OpenSubscriptionReward();

            PlayerData playerData = GameServices.Instance.PlayerStatisticService.PlayerData;
            playerData.CurrentSkin = IngameData.Settings.subscriptionRewardSettings.ShooterSkinTypes.FirstObject();
            playerData.SetWeaponSkin(WeaponType.Sniper, IngameData.Settings.subscriptionRewardSettings.WeaponSkinTypes.FirstObject());
        }


        private void SetUiKitCameraEnabled(bool value)
        {
            UiCamera.Instance.Camera.enabled = !value;

            IUiManager uiManager = Services.GetService<IUiManager>();
            uiManager.UiCamera.enabled = value;
        }

        #endregion
    }
}
