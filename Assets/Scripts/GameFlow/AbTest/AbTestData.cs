using System;
using System.Collections.Generic;
using Drawmasters.AbTesting;
using Drawmasters.OffersSystem;
using Drawmasters.Proposal;
using Drawmasters.Ui.Enums;
using Drawmasters.ServiceUtil;
using Drawmasters.Levels.Order;
using Modules.General.Abstraction;
using Newtonsoft.Json;


namespace Drawmasters
{
    [Serializable]
    public class InterstitialDynamicSettings
    {
        public float[] Sequence = { 0, 30, 45 };
        public float[] Cycled = { 0 };
    }


    public class AbTestData : IAbTestData
    {
        #region Drawmaster tests

        public bool isInfinityDrawingEnabled = true;
        public bool isUsedAlternativeLevels = true;

        public bool isBossLevelsEnabled = false;

        public LevelsOrder.AbTestReplaceData[] abTestSublevelsReplaceData = { };

        #endregion



        #region Iaps

        public bool isIapsAvailable = true;

        [JsonProperty("isNoAdsABTestEnabled")] // for general match
        public bool isNoAdsAvailable = true;

        #endregion



        #region Subscription

        public bool isSubscriptionAvailable = true;

        public float subscriptionReward = 2000.0f;
        public float subscriptionPremiumCurrencyReward = 200.0f;
        
        public bool isSubscriptionWidgetAvailable = true;

        public SubscriptionOfferSettings subscriptionOfferSettings = new SubscriptionOfferSettings()
        {
            IsAvailable = true,
            DurationTime = 300.0f,
            CooldownTime = 60.0f,
            IapID = "diamondweekly.sale",
            Triger = "custom",
            minLevelForShow = 5
        };

        public SubscriptionOfferSettings subscriptionWidgetOfferSettings = new SubscriptionOfferSettings()
        {
            IsAvailable = true,
            CooldownTime = 60.0f,
            IapID = "diamondweekly",
            Triger = "custom",
            minLevelForShow = 5
        };

        #endregion



        #region Notifications

        [JsonProperty("show_ask_notifications_after_level")]
        public int showAskNotificationsAfterLevel = 2;

        public bool isNotificationsEnabled = true;

        #endregion



        #region Progress
        //[JsonProperty("reach_modes_data")]
        [JsonIgnore]
        public string reachModesData = @"[{""mode"":2,""completeChapters"":0},{""mode"":3,""completeChapters"":3},{""mode"":4,""completeChapters"":5},{""mode"":1,""completeChapters"":7},{""mode"":6,""completeChapters"":9}]";

        [JsonProperty("reach_skin_factor")]
        public float reachSkinFactor = 0.1f;

        [JsonProperty("premium_currency_per_boss_level_complete")]
        public float premiumCurrencyPerBossLevelComplete = 30.0f;

        #endregion



        #region Forcemeter proposal

        [JsonProperty("min_level_for_forcemeter")]
        public int minFinishedLevelsForForcemeterPropose = 5;

        [JsonProperty("levels_delta_for_forcemeter")]
        public int finishedLevelsCountForForcemeterPropose = 9;

        public bool isForceMeterProposalAvailable = true;

        #endregion



        #region Shop result

        [JsonProperty("min_level_for_shop_result")]
        public int minFinishedLevelsForShopResultPropose = 7;

        [JsonProperty("levels_delta_for_shop_result")]
        public int finishedLevelsCountForShopResultPropose = 9;

        public bool isShopResultProposalEnabled = true;

        #endregion



        #region Premium Shop result

        [JsonProperty("min_level_for_premium_shop_result")]
        public int minFinishedLevelsForPremiumShopResultPropose = 12;

        [JsonProperty("levels_delta_for_premium_shop_result")]
        public int finishedLevelsCountForPremiumShopResultPropose = 9;

        [JsonProperty("is_premium_shop_result_proposal_enabled")]
        public bool isPremiumShopResultProposalEnabled = true;

        #endregion



        #region Roulette result

        [JsonProperty("min_level_for_roulette")]
        public int minFinishedLevelsForRoulettePropose = 2;

        [JsonProperty("levels_delta_for_roulette")]
        public int finishedLevelsCountForRoulettePropose = 9;

        public bool isRouletteResultProposalEnabled = true;

        #endregion



        #region Result screen bonus

        public int currencyBonusLevelsDelta = 0;

        public bool isAllowedCurrencyBonus = true;

        public int rewardMultipler = 5;
        public int standardRewardTimer = 2;
        public int minRewardMultiplier = 2;
            
        #endregion



        #region Spin roulette

        public float FreeRouletteSpinSeconds = 43200.0f;

        public int LevelToForceShowRouletteSpin = 2;

        public bool isSpinRouletteProposalAvailable = true;

        #endregion



        #region Video skin

        public bool isVideoSkinProposalAvailable = true;

        #endregion



        #region Menu

        //[JsonProperty("menu_return_type")]
        [JsonIgnore]
        public MenuReturnType menuReturnType = MenuReturnType.ReturnAfterEveryLevel;
        
        public MainMenuScreenState mainMenuScreenState = MainMenuScreenState.CombinedCollection;

        #endregion 



        #region Shooter skin proposal

        public bool isVideoShooterSkinProposalAvailable = true;

        public float videoShooterSkinProposalCooldown = 7200.0f;

        public int videoShooterSkinProposalMinLevel = 3;

        #endregion



        #region Weapon skin proposal

        public bool isVideoWeaponSkinProposalAvailable = true;

        public float videoWeaponSkinProposalCooldown = 7200.0f;

        public int videoWeaponSkinProposalMinLevel = 3;

        #endregion


        #region Bonus level

        //[JsonProperty("is_bonus_level_for_ads")]
        [JsonIgnore]
        public bool isBonusLevelForAds = false;

        //[JsonProperty("killed_enemies_for_bonus_level_propose")]
        [JsonIgnore]
        public int killedEnemiesForBonusLevelPropose = 35;

        [JsonIgnore]
        public bool isBonusLevelProposalAvailable = true;

        [JsonIgnore]
        public int bonusLevelProposalMinLevel = 0;

        #endregion



        #region Skip level proposal

        [JsonIgnore]
        public bool isSkipLevelProposalEnabled = true;

        [JsonIgnore]
        public bool isSkipLevelNeedRewardVideo = true;

        #endregion



        #region Common Proposal

        public float proposalSkipShowDelay = 2.0f;

        public float forcemeterSkipShowDelay = 0.2f;

        #endregion



        #region Mansion

        public bool isMansionAvailable = true;

        public float mansionHammersProposeSecondsCooldown = 0.0f;

        [JsonProperty("mansion_open_utc_date_time")]
        public string mansionOpenUtcDateTime = "2020-05-27T00:00:00.332Z";

        [JsonProperty("mansion_income_cooldown")]

        public float[] mansionIncomeCooldown =
            {
              4f * 60f * 60f,
              4f * 60f * 60f,
              4f * 60f * 60f,
              4f * 60f * 60f
            }; // 1 - 3

        [JsonProperty("min_level_for_mansion")]
        public int minLevelForMansion = 8;

        [JsonProperty("mansion_rooms_rewards")]
        public Dictionary<int, CurrencyRewardData> mansionRoomsRewards = new Dictionary<int, CurrencyRewardData>
        {
            { 0, new CurrencyRewardData(CurrencyType.Premium, 12) },
            { 1, new CurrencyRewardData(CurrencyType.Premium, 20) },
            { 2, new CurrencyRewardData(CurrencyType.Premium, 28) },
            { 3, new CurrencyRewardData(CurrencyType.Premium, 40) },
        };

        #endregion



        #region Monopoly

        public bool isMonopolyAvailable = true;

        public int minLevelForMonopoly = 12;

        public float monopolyDurationSeconds = 172800.0f;

        public bool monopolySpinOffUseReloadTimer = true;

        public float monopolyBonesForVideoReloadSeconds = 0.0f;

        public LiveOpsReloadTime monopolyReloadTime = new LiveOpsReloadTime() { day = 1, hours = 10, minutes = 0 };

        public float monopolyNotificationSecondsBeforeLiveOpsFinish = 86400.0f;

        public ProposalAvailabilitySettings monopolyAvailabilitySettings =
            new ProposalAvailabilitySettings()
            {
                cooldownLevels = 0,
                cooldownSeconds = 0,
                triggerDay = 0
            };

        #endregion



        #region HitmastersSpinOff

        public bool isHitmastersSpinOffAvailable = true;

        public int minLevelForHitmastersSpinOff = 21;

        public float hitmastersSpinOffDurationSeconds = 86400.0f;

        public bool hitmastersSpinOffUseReloadTimer = true;

        public LiveOpsReloadTime hitmastersReloadTime = new LiveOpsReloadTime() { day = 1, hours = 10, minutes = 0 };

        public float hitmastersNotificationSecondsBeforeLiveOpsFinish = 86400.0f;

        public string[] hitmastersSpinOffModesSequence = { GameMode.HitmastersLegacyShotgun.ToString(),
                                                           GameMode.HitmastersLegacySniper.ToString(),
                                                           GameMode.HitmastersLegacyGravygun.ToString(),
                                                           GameMode.HitmastersLegacyPortalgun.ToString() };

        public int[] hitmastersSpinOffForcemeterIndexes = { 5 };

        public int[] hitmastersSpinOffRouletteIndexes = { 13, 17 };

        public int[] hitmastersSpinOffPremiumShopIndexes = { 8 };

        public int[] hitmastersSpinOffBossIndexes = { 10, 20 };

        public ProposalAvailabilitySettings hitmastersLiveOpsAvailabilitySettings =
            new ProposalAvailabilitySettings()
            {
                cooldownLevels = 0,
                cooldownSeconds = 0.0f,
                triggerDay = 0
            };

        #endregion



        #region Season Event

        // TODO: this should accumulate all params below, but can't do that now that cuz of already started ab tests
        public SeasonEventAbSettings seasonEventAbSettings = new SeasonEventAbSettings()
        {
            LockIfPreviousUnclaimed = true,
            PointsPerLevel = 3,
            PointsPerStepVariantIndex = 0,
            RewardsVariantIndex = 0,
            ReturnType = SeasonEventReturnType.ReturnWhenRewardReached,
            AdModule = AdModule.RewardedVideo.ToString(),
            GoldenTicketLock = false
        };

        public bool isSeasonEventAvailable = true;

        public int minLevelForSeasonEvent = 25;

        public float seasonEventDurationSeconds = 172800.0f;

        public bool seasonEventUseReloadTimer = true;

        public LiveOpsReloadTime seasonEventReloadTime = new LiveOpsReloadTime() { day = 1, hours = 10, minutes = 0 };

        public float seasonEventNotificationSecondsBeforeLiveOpsFinish = 86400.0f;

        public ProposalAvailabilitySettings seasonEventAvailabilitySettings =
            new ProposalAvailabilitySettings()
            {
                cooldownLevels = 0,
                cooldownSeconds = 0,
                triggerDay = 0
            };

        public HappyHoursSettingsSeasonEvent happyHoursSettingsSeasonEvent =
            new HappyHoursSettingsSeasonEvent()
            {
                IsAvailable = true,
                DurationSeconds = 57600.0f,
                StartSecondsBeforeLiveOpsFinish = 57600.0f,

                PlayerPointsMultiplier = 2.0f
            };

        #endregion



        #region League

        public bool isLeagueAvailable = true;

        public int minLevelForLeague = 7;

        public float leagueDurationSeconds = 86400.0f;

        public bool leagueUseReloadTimer = false;

        public LiveOpsReloadTime leagueReloadTime =
            new LiveOpsReloadTime()
            {
                day = 0,
                hours = 0,
                minutes = 0
            };

        public float leagueNotificationSecondsBeforeLiveOpsFinish = 86400.0f;

        public ProposalAvailabilitySettings leagueAvailabilitySettings =
            new ProposalAvailabilitySettings()
            {
                cooldownLevels = 0,
                cooldownSeconds = 0,
                triggerDay = 0
            };

        public HappyHoursSettingsLeague happyHoursSettingsLeague = new HappyHoursSettingsLeague()
        {
            IsAvailable = true,
            DurationSeconds = 28800.0f,
            StartSecondsBeforeLiveOpsFinish = 28800.0f,
            PlayerSkullsMultiplier = 2.0f,
            BotsSkullsMultiplier = 1.5f
        };

        public bool leagueAvailableOffline = false;
            
        #endregion
        
        
        
        #region GoldenTicket

        public GoldenTicketOfferSettings goldenTicketOfferSettings = new GoldenTicketOfferSettings()
        {
            IsAvailable = true,
            DurationTime = 600.0f,
            CooldownTime = 60.0f,
            IapID = "golden.ticket",
            Triger = "after_min_step_claimed",
            minSeasonEventStepClaimed = 2,
            minTimeForStart = 600
        };

        #endregion


        #region Rate us

        public RateUsService.AbTestParams rateUsData = new RateUsService.AbTestParams()
        {
            isCustomRateUsAvailable = true,
            isIosNativeRateUsAvailalbe = true,
            isRewardAvailable = true
        };

        #endregion
    }
}
