using System;
using System.Collections.Generic;
using System.Linq;
using Drawmasters.OffersSystem;
using Drawmasters.Proposal;
using Drawmasters.Proposal.Interfaces;
using Drawmasters.ServiceUtil.Interfaces;
using Drawmasters.Statistics;
using Drawmasters.Utils;


namespace Drawmasters.ServiceUtil
{
    public class ProposalService : IProposalService
    {
        #region Ctor

        public ProposalService(IMusicService musicService,
                               ICommonStatisticsService commonStatistic,
                               IAbTestService abTestService,
                               ILevelEnvironment levelEnvironment,
                               IRateUsService rateUsService,
                               ILevelControllerService levelControllerService,
                               IShopService shopService,
                               IPlayerStatisticService playerStatisticService,
                               ITimeValidator timeValidator)
        {
            RouletteRewardController = new RouletteRewardController(IngameData.Settings.rouletteRewardSettings,
                                                                    PrefsKeys.Proposal.RouletteLevelsCounter,
                                                                    PrefsKeys.AbTest.UaRouletteAllow,
                                                                    PrefsKeys.AbTest.UaRouletteDeltaCount,
                                                                    abTestService,
                                                                    levelEnvironment);

            ShopResultController = new ShopResultController(IngameData.Settings.shopResultSettings,
                                                            PrefsKeys.Proposal.ShopResultLevelsCounter,
                                                            PrefsKeys.AbTest.UaShopResultAllow,
                                                            PrefsKeys.AbTest.UaShopResultDeltaCount,
                                                            abTestService,
                                                            levelEnvironment);

            PremiumShopResultController = new PremiumShopResultController(IngameData.Settings.premiumShopResultSettings,
                                                                          PrefsKeys.Proposal.PremiumShopResultLevelsCounter,
                                                                          PrefsKeys.AbTest.UaPremiumShopResultAllow,
                                                                          PrefsKeys.AbTest.UaPremiumShopResultDeltaCount,
                                                                          abTestService,
                                                                          levelEnvironment);

            IngameCurrencyMultiplier = new ResultRewardController(PrefsKeys.Proposal.ResultRewardLevelsCounter,
                                                                  PrefsKeys.AbTest.UaLevelsResultRewardAllow,
                                                                  PrefsKeys.AbTest.UaLevelsResultRewardDeltaCount,
                                                                  abTestService,
                                                                  levelEnvironment);

            RateUsProposal = new RateUsProposal(commonStatistic, rateUsService, abTestService);

            SpinRouletteController = new SpinRouletteController(IngameData.Settings.spinRouletteSettings,
                                                                musicService,
                                                                abTestService,
                                                                commonStatistic,
                                                                timeValidator);

            ForceMeterController = new ForceMeterController(IngameData.Settings.forceMeterRewardPackSettings,
                                                            PrefsKeys.Proposal.ForcemeterLevelsCounter,
                                                            PrefsKeys.AbTest.UaForcemeterAllow,
                                                            PrefsKeys.AbTest.UaForcemeterDeltaCount,
                                                            abTestService,
                                                            levelEnvironment);

            LevelSkipProposal = new LevelSkip();

            VideoShooterSkinProposal = new VideoShooterSkin(IngameData.Settings.shooterSkinPanelsSettings,
                                                            abTestService, commonStatistic);

            VideoWeaponSkinProposal = new VideoWeaponSkin(IngameData.Settings.weaponSkinPanelsSettings,
                                                           abTestService,
                                                           commonStatistic);

            CurrencyShopProposal = new CurrencyShopProposal();

            SkinProposal = new SkinProposalStatistic(levelEnvironment, abTestService, shopService);

            MansionRewardController = new MansionRewardController(timeValidator);
            MansionProposeController = new MansionProposeController();

            UiPanelShooterSkinRewardController = new UiPanelRewardController(IngameData.Settings.shooterSkinPanelsSettings,
                                                                             PrefsKeys.Proposal.UiPanelShooterSkinShowsCount);
            UiPanelWeaponSkinRewardController = new UiPanelRewardController(IngameData.Settings.weaponSkinPanelsSettings,
                                                                             PrefsKeys.Proposal.UiPanelWeaponSkinShowsCount);

            MonopolyProposeController = new MonopolyProposeController(IngameData.Settings.monopoly.settings,
                                                                      IngameData.Settings.monopoly.visualSettings,
                                                                      abTestService,
                                                                      commonStatistic,
                                                                      playerStatisticService,
                                                                      timeValidator);

            HitmastersProposeController = new HitmastersProposeController(IngameData.Settings.hitmasters.settings,
                                                                          IngameData.Settings.hitmasters.visualSettings,
                                                                          IngameData.Settings.hitmasters.liveOpsLevelsOrder,
                                                                          abTestService,
                                                                          commonStatistic,
                                                                          playerStatisticService,
                                                                          timeValidator);

            SeasonEventProposeController = new SeasonEventProposeController(IngameData.Settings.seasonEvent.seasonEventSettings,
                                                                            IngameData.Settings.seasonEvent.seasonEventVisualSettings,
                                                                            abTestService,
                                                                            commonStatistic,
                                                                            playerStatisticService,
                                                                            timeValidator,
                                                                            shopService,
                                                                            shopService.ShooterSkins,
                                                                            shopService.WeaponSkins,
                                                                            shopService.PetSkins);

            LeagueProposeController = new LeagueProposeController(IngameData.Settings.league.leagueSettings,
                                                                            IngameData.Settings.league.leagueVisualSettings,
                                                                            abTestService,
                                                                            commonStatistic,
                                                                            playerStatisticService,
                                                                            timeValidator);

            HappyHoursLeagueProposeController = new HappyHoursLeagueProposeController(LeagueProposeController,
                                                                                      IngameData.Settings.league.happyHoursVisualSettings,
                                                                                      abTestService.CommonData.happyHoursSettingsLeague,
                                                                                      abTestService,
                                                                                      commonStatistic,
                                                                                      playerStatisticService,
                                                                                      timeValidator);

            HappyHoursSeasonEventProposeController = new HappyHoursSeasonEventProposeController(SeasonEventProposeController,
                                                                                                IngameData.Settings.seasonEvent.happyHoursVisualSettings,
                                                                                                abTestService.CommonData.happyHoursSettingsSeasonEvent,
                                                                                                abTestService,
                                                                                                commonStatistic,
                                                                                                playerStatisticService,
                                                                                                timeValidator);


            offers = new Dictionary<Type, BaseOffer>()
            {
                {
                    typeof(SubscriptionOffer), new SubscriptionOffer(abTestService, commonStatistic,
                        playerStatisticService,
                        timeValidator)
                },
                {
                    typeof(SubscriptionWidgetOffer), new SubscriptionWidgetOffer(abTestService, commonStatistic,
                        playerStatisticService,
                        timeValidator)
                },
                {
                    typeof(GoldenTicketOffer), new GoldenTicketOffer(
                        abTestService, 
                        SeasonEventProposeController,
                        HappyHoursSeasonEventProposeController,
                        commonStatistic,
                        playerStatisticService,
                        timeValidator)
                }
            };
        }

        #endregion

        private readonly Dictionary<Type, BaseOffer> offers;


        public T GetOffer<T>() where T : BaseOffer =>
            offers.First(e => e.Key == typeof(T)).Value as T;

        public T[] GetOffers<T>() where T: class =>
            offers.Select(e => e.Value).OfType<T>().ToArray();



        #region IProposalService

        public ResultRewardController IngameCurrencyMultiplier { get; }

        public RouletteRewardController RouletteRewardController { get; }

        public ShopResultController ShopResultController { get; }

        public PremiumShopResultController PremiumShopResultController { get; }

        public RateUsProposal RateUsProposal { get; }

        public SpinRouletteController SpinRouletteController { get; }

        public ForceMeterController ForceMeterController { get; }

        public IProposable LevelSkipProposal { get; }

        public IProposable VideoPetSkinProposal { get; }

        public IProposable VideoShooterSkinProposal { get; }

        public IProposable VideoWeaponSkinProposal { get; }

        public CurrencyShopProposal CurrencyShopProposal { get; }

        public SkinProposalStatistic SkinProposal { get; }

        public MansionProposeController MansionProposeController { get; }

        public MansionRewardController MansionRewardController { get; }

        public UiPanelRewardController UiPanelShooterSkinRewardController { get; }

        public UiPanelRewardController UiPanelWeaponSkinRewardController { get; }

        public MonopolyProposeController MonopolyProposeController { get; }

        public HitmastersProposeController HitmastersProposeController { get; }

        public SeasonEventProposeController SeasonEventProposeController { get; }

        public LeagueProposeController LeagueProposeController { get; }

        public HappyHoursLeagueProposeController HappyHoursLeagueProposeController { get; }

        public HappyHoursSeasonEventProposeController HappyHoursSeasonEventProposeController { get; }

        #endregion
    }
}
