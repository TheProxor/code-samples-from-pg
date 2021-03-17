using System;
using Modules.InAppPurchase;
using UnityEngine;
using Drawmasters.Announcer;
using Drawmasters.Levels;
using Drawmasters.Vibration;
using Drawmasters.Advertising;
using Drawmasters.Tutorial;
using Drawmasters.Proposal;
using Drawmasters.Levels.Order;
using Drawmasters.Ui;
using Drawmasters.Mansion;
using Drawmasters.Pets;
using Drawmasters.OffersSystem;
using Drawmasters.Proposal.Settings;


namespace Drawmasters
{
    [CreateAssetMenu(fileName = "IngameData",
                     menuName = NamingUtility.MenuItems.Singletons + "IngameData")]
    public class IngameData : ScriptableSingleton<IngameData>
    {
        #region Helper classes

        [Serializable]
        public class Monopoly
        {
            public MonopolySettings settings = default;
            public MonopolyVisualSettings visualSettings = default;
        }


        [Serializable]
        public class Hitmasters
        {
            public LevelsOrder liveOpsLevelsOrder = default;
            public CurrencyAnnouncerSettings currencyAnnouncerSettings = default;
            public HitmastersSettings settings = default;
            public HitmastersVisualSettings visualSettings = default;
        }

        [Serializable]
        public class SeasonEvent
        {
            public SeasonEventSettings seasonEventSettings = default;
            public SeasonEventVisualSettings seasonEventVisualSettings = default;
            public SequenceRewardPackSettings forceMeterSeasonEventRewardPackSettings = default;
            public SpinRouletteSettings spinRouletteCashSeasonEventRewardPackSettings = default;
            public SpinRouletteSettings spinRouletteSkinSeasonEventRewardPackSettings = default;
            public SpinRouletteSettings spinRouletteWaiponSeasonEventRewardPackSettings = default;

            public HappyHoursVisualSettings happyHoursVisualSettings = default;
        }


        [Serializable]
        public class League
        {
            public LeagueSettings leagueSettings = default;
            public LeagueVisualSettings leagueVisualSettings = default;
            public LeagueRewardSettings leagueRewardSettings = default;
            public BotSettings botSettings = default;
            public ChestSettings chestSettings = default;

            public HappyHoursVisualSettings happyHoursVisualSettings = default;
        }


        [Serializable]
        public class Offers
        {
            public GoldenTicketOfferSettings goldenTicketOfferSettings = default;
            
        }


        [Serializable]
        public class Pets
        {
            public PetSkinsSettings skinsSettings = default;
            public PetAnimationSettings animationSettings = default;
            public PetRenderSeparatorSettings renderSeparatorSettings = default;
            public PetUiSettings uiSettings = default;
            public PetLevelSettings levelSettings = default;
            public PetWeaponSkinSettings weaponSkinSettings = default;
        }


        [Serializable]
        public class SettingsGroup
        {
            public GameModesInfo modesInfo = default;
            public ShooterSettings shooter = default;
            public CommonLevelSettings level = default;
            public CommonPhysicalObjectsSettings physicalObject = default;
            public CommonVisualObjectsSettings commonVisualObjectsSettings = default;
            public CommonLevelTargetSettings levelTarget = default;
            public CommonMonolithSettings monolith = default;
            public DynamiteSettings dynamiteSettings = default;
            public LevelTargetSettings levelTargetSettings = default;
            public LiquidSettings liquidSettings = default;
            public CommonSpikesSettings commonSpikesSettings = default;
            public PerfectsSettings currencySettings = default;
            public VibrationSettings vibrationSettings = default;
            public AnnouncerSettings announcerSettings = default;
            public AnnouncerSettings skullAnnouncerSettings = default;
            public AdsSettings adsSettings = default;
            public TutorialSettings tutorialSettings = default;
            public ShooterSkinsSettings shooterSkinsSettings = default;
            public WeaponSkinSettings weaponSkinSettings = default;
            public LevelWinMotionSettings levelWinMotionSettings = default;
            public CommonBackgroundsSettings commonBackgroundsSettings = default;
            public RouletteRewardPackSettings rouletteRewardSettings = default;
            public SingleRewardPackSettings shopResultSettings = default;
            public SingleRewardPackSettings premiumShopResultSettings = default;
            public CameraShakeSettings cameraShakeSettings = default;
            public LevelsOrder levelsOrder = default;
            public TransitionSettings transitionSettings = default;
            public ShooterAimingSettings shooterAimingSettings = default;
            public ColorProfilesSettings colorProfilesSettings = default;
            public BossLevelSettings bossLevelSettings = default;
            public CommonRewardSettings commonRewardSettings = default;
            public BossLevelTargetSettings bossLevelTargetSettings = default;
            public ProjectileSkinsSettings projectileSkinsSettings = default;
            public DialogPopupSettings dialogPopupSettings = default;
            public PortalsSettings portalsSettings = default;
            public IngameCameraSettings ingameCameraSettings = default;
            public SubscriptionRewardSettings subscriptionRewardSettings = default;
            public LaserSettings laserSettings = default;
            public SpinRouletteSettings spinRouletteSettings = default;
            public ForceMeterRewardPackSettings forceMeterRewardPackSettings = default;
            public AdsSkinPanelsSettings shooterSkinPanelsSettings = default;
            public AdsSkinPanelsSettings weaponSkinPanelsSettings = default;
            public CommonDynamicSettings commonDynamicSettings = default;
            public IapsRewardSettings iapsRewardSettings = default;
            public ForceMeterUiSettings forceMeterUiSettings = default;
            public ShooterAnimationSettings shooterAnimationSettings = default;
            public MansionRewardPackSettings mansionRewardPackSettings = default;
            public CoinLevelObjectSettings coinLevelObjectSettings = default;

            // drawmasters
            public ProjectileSmashSettings projectileSmashSettings = default;
            public ShooterTrajectoryDrawSettings trajectoryDrawSettings = default;
            public RocketLaunchDrawSettings rocketLaunchDrawSettings = default;
            public SkinsColorSettings skinsColorSettings = default;
            public LevelTargetSkinsSettings levelTargetSkinsSettings = default;
            public LevelGraphicSettings levelGraphicSettings = default;
            public ProjectileSmashShakeSettings projectileSmashShakeSettings = default;
            public LevelTargetAnimationNamesSettings levelTargetAnimationNamesSettings = default;
            public BonusLevelSettings bonusLevelSettings = default;
            public SkipAnimationSettings skipAnimationSettings = default;
            public ShooterSkinsFxsSettings shooterSkinsFxsSettings = default;

            public Monopoly monopoly = default;
            public Hitmasters hitmasters = default;
            public SeasonEvent seasonEvent = default;
            public League league = default;
            public Offers offers = default;
            public Pets pets = default;

            public CommonContentSettings commonContentSettings = default; // TODO: maybe delete field, if we don't need this in build
            public LinkedMarkerLevelObjectSettings linkedMarkerLevelObjectSettings = default;
        }

        #endregion


        #region Fields

        [SerializeField] private SettingsGroup settings = default;

        #endregion
        


        #region Properties

        public static SettingsGroup Settings => Instance.settings;


        public static void Initialize()
        {
            Settings.shooterSkinPanelsSettings.Initialize();
            Settings.weaponSkinPanelsSettings.Initialize();
            Settings.subscriptionRewardSettings.Initialize();
        }

        #endregion
    }
}
