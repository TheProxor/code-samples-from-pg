using System;
using System.Collections.Generic;
using Drawmasters.ServiceUtil;
using Drawmasters.ServiceUtil.Interfaces;
using Modules.Advertising;
using Modules.General.Abstraction;
using Modules.Sound;


namespace Drawmasters.Advertising
{
    public class CustomAdsManager : AdvertisingManager
    {
        #region Fields

        private IAdvertisingNecessaryInfo advertisingNecessaryInfo;
        private IAdsHelperService adsHelper;
        
        protected AdsWrapper adsWrapper = new AdsWrapper();

        #endregion



        #region Properties

        private IAdsHelperService AdsHelper
        {
            get
            {
                if (adsHelper == null && GameServices.IsInstanceExist)
                {
                    adsHelper = GameServices.Instance.AdsHelperService;
                }

                return adsHelper;
            }
        }
        //
        // // TODO: ask for transfer on plugin side
        // public static bool IsInstantShowRewardedVideoCheat { get; set; }

        #endregion



        #region Public methods

        public override void Initialize(IAdvertisingService[] adServices,
                                        IAdvertisingNecessaryInfo advertisingInfo,
                                        IAdvertisingAbTestData[] abTestData,
                                        List<AdAvailabilityParameter> adAvailabilityParameters = null,
                                        List<AdPlacementModel> advertisingPlacements = null)
        {
            base.Initialize(adServices, advertisingInfo, abTestData, adAvailabilityParameters, advertisingPlacements);

            advertisingNecessaryInfo = advertisingInfo;
        }


        public override void TryShowAdByModule(AdModule module, string adShowingPlacement, Action<AdActionResultType> callback = null)
        {
            if (adsWrapper.IsAvailableForModule(module))
            {
                adsWrapper.ShowAdsTryShowAdByModule(module, adShowingPlacement, callback);
            }
            else
            {
                #warning wrong logic if banner exists
                SetControlsAvailability(false);

                callback += result =>
                {
                    bool wasAdsShown = result == AdActionResultType.Skip ||
                                       result == AdActionResultType.Success;
                    if (!wasAdsShown)
                    {
                        SetControlsAvailability(true);
                    }
                };

                base.TryShowAdByModule(module, adShowingPlacement, callback);
            }
        }

        #endregion



        #region Private methods

        private void SetControlsAvailability(bool isControlsActive)
        {
            //EventSystemController.SetSystemEnabled(isControlsActive, this);
        }


        private void MarkAdsActive(bool isAdsActive)
        {
            SoundManager.MuteAll(isAdsActive);
            GameManager.Instance.SetGamePaused(isAdsActive, this);
        }

        #endregion



        #region Protected methods

        public override bool IsAdModuleByPlacementAvailable(AdModule adModule, string placementName)
        {
            bool result = base.IsAdModuleByPlacementAvailable(adModule, placementName);

            if (adModule == AdModule.Banner)
            {
                result = false;
            }
            else if (adModule == AdModule.Interstitial)
            {

                result &= AdsHelper?.AdsLimiter.CanProposeInterstitial ?? false;
                result &= AdsHelper?.AdsLimiter.IsEnoughTimePassed ?? false;

                if (result)
                {
                    result &= IsInterstitialWithoutTimeAndMinLevelLimitAvailable(placementName);
                }
            }

            return result;
        }


        private bool IsInterstitialWithoutTimeAndMinLevelLimitAvailable(string placementName)
        {
            bool result = true;

            if (IsCrossPromoInterstitialAvailable(placementName))
            {
                result &= IsCrossPromoInterstitialAvailable(placementName);
            }
            else
            {
                result &= !advertisingNecessaryInfo.IsSubscriptionActive;
                result &= !advertisingNecessaryInfo.IsNoAdsActive;
                result &= !advertisingNecessaryInfo.IsPersonalDataDeleted;

                result &= !IsSubscriptionShowing;

                if (result)
                {
                    result &= IsInterstitialAvailable(placementName);
                }
            }

            return result;
        }

        #endregion



        #region Events handlers

        protected override void Controller_OnAdHide(
            AdModule adModule,
            AdActionResultType responseResultType,
            string errorDescription,
            string adIdentifier)
        {
            base.Controller_OnAdHide(adModule, responseResultType, errorDescription, adIdentifier);

            switch (adModule)
            {
                case AdModule.Interstitial:
                case AdModule.RewardedVideo:
                    MarkAdsActive(false);
                    break;
            }
        }


        protected override void Controller_OnAdShow(AdModule adModule,
                                                    AdActionResultType responseResultType,
                                                    int delay,
                                                    string errorDescription,
                                                    string adIdentifier,
                                                    string advertisingPlacement)
        {
            base.Controller_OnAdShow(adModule,
                                     responseResultType,
                                     delay,
                                     errorDescription,
                                     adIdentifier,
                                     advertisingPlacement);

            if (responseResultType == AdActionResultType.Success)
            {
                SetControlsAvailability(true);

                MarkAdsActive(true);
            }

            // TODO rare case (ask valery.p)
            if (responseResultType == AdActionResultType.Skip)
            {
                SetControlsAvailability(true);
            }
        }

        #endregion
    }
}
