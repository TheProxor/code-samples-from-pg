using System;
using System.Collections.Generic;
using AbTest;
using AbTest.ApiClient.V2;
using CrossPromo;
using Drawmasters.AbTesting;
using Drawmasters.Advertising;
using Drawmasters.Levels;
using Drawmasters.Ua;
using Drawmasters.Ui;
using Drawmasters.Utils;
using Drawmasters.Vibration;
using Modules.AbTest;
using Modules.Advertising;
using Modules.Analytics;
using Modules.AppsFlyer;
using Modules.Firebase;
using Modules.General;
using Modules.General.Abstraction;
using Modules.General.InAppPurchase;
using Modules.General.InitializationQueue;
using Modules.MoPub;
using Modules.Notification;
using Modules.Privacy;
using Spine.Unity.Examples;
using UnityEngine;
using UnityEngine.EventSystems;
using Modules.UiKit;


namespace Drawmasters
{
    public static class InitializationUtil
    {
        public static void Initialize(Action onInitialized, GameObject privacyPluginPrefab, GameObject uiKitManagerPrefab)
        {
            var mopubAbTestSettings = new MoPubAbTestKeysData();
            var ingameAbTestSettings = new IngameAdsAbTestData();
            var crossPromoAbTestSettings = new CrossPromoAbTestData();
            var samplingAbTestSettings = new SamplingAbTestData();
            var abTestPopupData = new AbTestPopupData();

            AnalyticsManagerSettings analyticsSettings = new AnalyticsManagerSettings()
            {
                Services = new IAnalyticsService[]
                {
                    new AppsFlyerAnalyticsServiceImplementor(LLAppsFlyerSettings.GetAppsFlyerSettings()),
                    new FirebaseAnalyticsServiceImplementor(null)
                }
            };

            Services.CreateServiceSingleton<IAnalyticsManagerSettings, AnalyticsManagerSettings>(analyticsSettings);

            var advertisingNecessaryInfo = new AdsNecessaryInfo();

            //HACK
            _ = CustomPlacementsHolder.wasLoaderHidden;

            var adsManagerSettings = new AdvertisingManagerSettings
            {
                AdvertisingInfo = advertisingNecessaryInfo,
                AdServices = new IAdvertisingService[]
                {

                    new EditorAdvertisingServiceImplementor(AdvertisingEditorSettings.Instance),

                    new MoPubAdvertisingServiceImplementor(mopubAbTestSettings),

                    new CrossPromoAdvertisingServiceImplementor(
                        advertisingNecessaryInfo,
                        crossPromoAbTestSettings,
                        new AppsFlyerCrossPromoTracker())
                },
                AbTestData = new IAdvertisingAbTestData[]
                {
                    ingameAbTestSettings,
                    crossPromoAbTestSettings
                },
                AdAvailabilityParameters = new List<AdAvailabilityParameter>()
                {
                    new AdAvailabilityParameter(AdModule.Interstitial,
                                                AdsInterstitialPlaceKeys.BetweenSublevels,
                                                CustomPlacementsHolder.IsBetweenSublevelsInterstitialAvailable),
                    new AdAvailabilityParameter(AdModule.Interstitial,
                                                AdsInterstitialPlaceKeys.BackgroundInterstitialParameter,
                                                CustomPlacementsHolder.WasLoaderHidden),
                    new AdAvailabilityParameter(AdModule.Interstitial,
                                                AdsInterstitialPlaceKeys.SeasonEventScreenReward,
                                                (_) => true)
                },
                AdvertisingPlacements = new List<AdPlacementModel>()
                {
                    new AdPlacementModel(AdModule.Interstitial,
                                         AdsInterstitialPlaceKeys.BetweenSublevels,
                                         AdPlacementType.DefaultPlacement,
                                         new[] { AdsInterstitialPlaceKeys.BetweenSublevels }),
                    new AdPlacementModel(AdModule.Interstitial,
                                         AdPlacementType.Background, 
                                         AdPlacementType.Background,
                                         new [] { AdsInterstitialPlaceKeys.BackgroundInterstitialParameter }),
                    new AdPlacementModel(AdModule.Interstitial,
                                         AdsInterstitialPlaceKeys.SeasonEventScreenReward,
                                         AdPlacementType.DefaultPlacement,
                                         new [] { AdsInterstitialPlaceKeys.SeasonEventScreenReward })
                }
            };

            Services.CreateServiceSingleton<IAdvertisingManagerSettings, AdvertisingManagerSettings>
                (adsManagerSettings);

            AbTestServiceSettings settings = new AbTestServiceSettings()
            {
                ProjectName = "drawmasters", //AbTestHelper.ProjectName,
                TestData = new IAbTestData[]
                {
                    new AbTestData(),
                    crossPromoAbTestSettings,
                    mopubAbTestSettings, //"MoPubKeys"
                    ingameAbTestSettings, //IngameAdsAbTestData.AbTestKey
                    samplingAbTestSettings,
                    abTestPopupData
                }
            };

            Services.CreateServiceSingleton<IAbTestServiceSettings, AbTestServiceSettings>(settings);

            Services.CreateServiceSingleton<IScheduler, Scheduler>(Scheduler.Instance);
            
            Services.CreateServiceSingleton<INotificationManager, NotificationManager>();

            StoreManager.Instance.Initialize(LLStoreSettings.StoreItems.ToArray());

            InitializationQueue.Instance
                .AddService<UiManager>(uiKitManagerPrefab)
                .AddService<UiAdsController>()
                .AddService<CustomAdsManager>()
                .AddService<AbTestService>()
                .AddService<AnalyticsManager>()
                .AddService<AnalyticsProcessor>()
                .AddService<SystemPopupService>()
                .AddService<LLPrivacyManager>(privacyPluginPrefab)
                .AddService<SamplingService>()
                .AddService<DataStateService>()
                .AddService<AtTrackingManager>()
                // .AddService<NotificationManager>()
                .SetOnComplete(onInitialized)
                .Apply();
        }


        public static void InitializeIngame()
        {
            Content.Storage.InitializePools();

            VibrationManager.Initialize();

            PerfectsManager.Initialize();
            IngameData.Initialize();

            IngameCamera.Instance.Initialize();
            UiCamera.Instance.Initialize();

            SkeletonRagdoll2D.AttachmentBoundingBoxNameMarker = string.Empty;
            EventSystemController.SetupEventSystem(EventSystem.current);
            TransformUtility.Initialize(UiCamera.Instance.Camera, Camera.main);

            ScreenChangesMonitor.Initialize();

            #if UNITY_EDITOR || DEBUG_TARGET || UA_BUILD
            new PuzzlemastersDevContent(IngameData.Settings.commonBackgroundsSettings.BackgroundNames,
                                            0,
                                            Color.white);
            #endif
        }


        public static void DeinitializeIngame()
        {
            VibrationManager.Deinitialize();
            PerfectsManager.Deinitialize();
            IngameCamera.Instance.Deinitialize();
            ScreenChangesMonitor.Deinitialize();
        }
    }
}

