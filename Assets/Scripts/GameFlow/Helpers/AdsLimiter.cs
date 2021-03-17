using System;
using Drawmasters.Ua;
using Drawmasters.AbTesting;
using UnityEngine;
using Drawmasters.ServiceUtil.Interfaces;
using Modules.Advertising;
using Modules.General.Abstraction;
using Modules.General.InAppPurchase;
using IAbTestService = Drawmasters.ServiceUtil.Interfaces.IAbTestService;

namespace Drawmasters.Advertising
{
    public class AdsLimiter
    {
        #region Fields

        private readonly IngameAdsAbTestData adsData;

        private readonly ICommonStatisticsService commonStatisticsService;

        #endregion



        #region Properties

        public bool IsEnoughTimePassed
        {
            get
            {
                bool result = true;

                // use Abs to prevent time cheating
                float timeSinceVideoWatched = Mathf.Abs((float)DateTime.UtcNow.Subtract(LastVideoWatchTime).TotalSeconds);
                float timeSinceInterstitialWatched = Mathf.Abs((float)DateTime.UtcNow.Subtract(LastInterstitialWatchTime).TotalSeconds);

                float interstitialCooldown;

                if (adsData.isInterstitialCooldownDynamic)
                {
                    if (InterstitialsWatchedCount > 0)
                    {
                        float[] cooldownData = UseCycleDynamicInterstitialsCooldown ?
                            adsData.dynamicInterstitialSettings.Cycled :
                            adsData.dynamicInterstitialSettings.Sequence;

                        int dataIndex;

                        if (UseCycleDynamicInterstitialsCooldown)
                        {
                            dataIndex = cooldownData.Length == 0 ? default : SessionCycleDataInterstitialsCount % cooldownData.Length;
                        }
                        else
                        {
                            dataIndex = InterstitialsWatchedCount - 1;
                        }

                        interstitialCooldown = cooldownData.SafeGet(dataIndex);
                    }
                    else
                    {
                        interstitialCooldown = 0.0f;
                    }
                }
                else
                {
                    interstitialCooldown = adsData.delayBetweenInterstitials;
                }

                bool isInterstitialCooldownPassed = timeSinceInterstitialWatched > interstitialCooldown;
                bool isVideoCooldownPassed = timeSinceVideoWatched > adsData.interstitialCooldownAfterVideo;

                result &= isVideoCooldownPassed;
                result &= isInterstitialCooldownPassed;

                return result;
            }
        }


        public bool CanProposeInterstitial
        {
            get
            {
                bool result = true;

                int finishedLevels = commonStatisticsService.LevelsFinishedCount;

                bool isMinLevelPassed = finishedLevels >= adsData.minLevelForInterstitialShowing;
                bool isUaBuild = BuildInfo.IsUaBuild;                
                bool hasAnyActiveSubscription = StoreManager.Instance.HasAnyActiveSubscription;

                result &= isMinLevelPassed;
                result &= !isUaBuild;                
                result &= !hasAnyActiveSubscription;

                return result;
            }
        }


        public bool CanProposeBanner
        {
            get
            {
                bool result = true;

  
                bool hasAnyActiveSubscription = StoreManager.Instance.HasAnyActiveSubscription;


                result &= !hasAnyActiveSubscription;

                return result;
            }
        }


        private bool UseCycleDynamicInterstitialsCooldown =>
            (InterstitialsWatchedCount - 1) > adsData.dynamicInterstitialSettings.Sequence.Length - 1;


        private DateTime LastInterstitialWatchTime
        {
            get => CustomPlayerPrefs.GetDateTime(PrefsKeys.Analytics.LastInterstitialWatchTime);
            set => CustomPlayerPrefs.SetDateTime(PrefsKeys.Analytics.LastInterstitialWatchTime, value);
        }


        private DateTime LastVideoWatchTime
        {
            get => CustomPlayerPrefs.GetDateTime(PrefsKeys.Analytics.LastVideoWatchTime);
            set => CustomPlayerPrefs.SetDateTime(PrefsKeys.Analytics.LastVideoWatchTime, value);
        }


        private int SessionCycleDataInterstitialsCount { get; set; }


        private int InterstitialsWatchedCount
        {
            get => CustomPlayerPrefs.GetInt(PrefsKeys.Analytics.InterstitialWatchCount);
            set => CustomPlayerPrefs.SetInt(PrefsKeys.Analytics.InterstitialWatchCount, value);
        }

        #endregion



        #region Ctor

        public AdsLimiter(IAbTestService abTestService,
                          ICommonStatisticsService _commonStatisticsService)
        {
            adsData = abTestService.AdsData;
            commonStatisticsService = _commonStatisticsService;

            AdvertisingManager.Instance.OnAdHide += Instance_OnAdHide;
        }


        #endregion



        #region Private methods

        private void MarkVideoWatched() => LastVideoWatchTime = DateTime.UtcNow;


        private void MarkInterstitialWatched()
        {
            //TODO hold current player level
            LastInterstitialWatchTime = DateTime.UtcNow;
            InterstitialsWatchedCount++;

            if (UseCycleDynamicInterstitialsCooldown)
            {
                SessionCycleDataInterstitialsCount++;
            }
        }

        #endregion



        #region Events handlers

        private void Instance_OnAdHide(AdModule module, AdActionResultType resultType)
        {
            switch(module)
            {
                case AdModule.Interstitial:
                    MarkInterstitialWatched();
                    break;
                case AdModule.RewardedVideo:
                    MarkVideoWatched();
                    break;
            }
        }

        #endregion
    }
}
