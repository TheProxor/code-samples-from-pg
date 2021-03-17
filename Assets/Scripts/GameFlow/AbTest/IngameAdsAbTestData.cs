using System.Collections.Generic;
using Modules.Advertising;
using Modules.General.Abstraction;
using Newtonsoft.Json;

namespace Drawmasters.AbTesting
{
    public class IngameAdsAbTestData : IInGameAdvertisingAbTestData
    {
        #region Fields

        [JsonIgnore] public const string AbTestKey = "AdsSettings";

        #endregion



        #region IInGameAdvertisingAbTestData

        public Dictionary<AdModule, float> advertisingAvailabilityEventInfo { get; set; } =
            new Dictionary<AdModule, float>()
            {
                { AdModule.RewardedVideo, 10.0f },
                { AdModule.Interstitial, 15.0f },
                { AdModule.Banner, 5.0f },
            };

        public int minLevelForBannerShowing { get; set; } = 0;

        public int minLevelForInterstitialShowing { get; set; } = 1;

        public float delayBetweenInterstitials { get; set; } = 30.0f;

        public int matchesBetweenInterstitials { get; set; } = 0;

        public bool isNeedShowInterstitialBeforeResult { get; set; } = true;

        public bool isNeedShowInterstitialAfterResult { get; set; } = true;

        public bool isNeedShowInterstitialAfterBackground { get; set; } = true;

        public bool isNeedShowInactivityInterstitial { get; set; } = true;

        public float delayBetweenInactivityInterstitials { get; set; } = 30.0f;

        public bool isNeedShowSettingsOpenInterstitials { get; set; } = true;

        public bool isNeedShowSettingsCloseInterstitials { get; set; } = false;

        public bool isNeedShowGalleryOpenInterstitials { get; set; } = false;

        public bool isNeedShowGalleryCloseInterstitials { get; set; } = false;

        public bool isNeedShowInGameRestartInterstitial { get; set; } = true;

        public bool isNeedShow9ChestInterstitial { get; set; } = false;

        public bool isNeedShowInterstitialBetweenLevelSegments { get; set; } = false;

        #endregion



        #region Custom parameters

        public float interstitialCooldownAfterVideo { get; set; } = 30.0f;

        public bool isDelayBetweenInterstitialsAfterBackgroundEnabled { get; set; } = true;

        public bool isNeedInterstitialsBetweenLevels { get; set; } = true;

        public bool isInterstitialCooldownDynamic { get; set; } = false;

        public InterstitialDynamicSettings dynamicInterstitialSettings { get; set; } =
            new InterstitialDynamicSettings();
        
        public bool isNeedShowInterstitialAfterSegment { get; set; } = false;

        #endregion
    }
}

