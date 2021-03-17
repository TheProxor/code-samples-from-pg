using Drawmasters.OffersSystem;
using Drawmasters.Pets;
using Drawmasters.ServiceUtil.Interfaces;
using Drawmasters.Utils;
using UnityEngine;


namespace Drawmasters.ServiceUtil
{
    public class GameServices
    {
        #region Fields

        private static GameServices instance;

        private static bool isInitialized;

        #endregion



        #region Properties

        public static bool IsInstanceExist => instance != null;

        public static GameServices Instance
        {
            get
            {
                if (!isInitialized)
                {
                    isInitialized = true;

                    instance = new GameServices();
                    instance.InitializeServices();
                }

                return instance;
            }
        }

        #endregion



        #region Services
        
        public ITimeServices Time { get; }
        
        public ILevelEnvironment LevelEnvironment { get;}

        public IMusicService MusicService { get; }

        public ILevelControllerService LevelControllerService { get;}

        public IBackgroundService BackgroundService { get; }

        public IAnalyticImplementationService AnalyticImplementationService { get; }
        
        public ICommonStatisticsService CommonStatisticService { get; }

        public IRateUsService RateUsService { get; }

        public IAbTestService AbTestService { get; }

        public ILevelOrderService LevelOrderService { get; }

        public IPlayerStatisticService PlayerStatisticService { get; }

        public IAdsHelperService AdsHelperService { get; }

        public IProposalService ProposalService { get; }

        public IShopService ShopService { get; }

        public ILevelGraphicService LevelGraphicService { get; }

        public INotificationService NotificationService { get; }

        public ProposalsLoader ProposalsLoader { get; }
        
        public IAnalyticTracker AnalyticTracker { get; }

        public ITimeValidator TimeValidator { get; }
        
        public IMemoryWarning MemoryWarning { get; }
        
        public IApplicationVersion ApplicationVersion { get; }

        public IPetsService PetsService { get; }

        #endregion



        #region Ctor

        private GameServices()
        {
            ApplicationVersion = new ApplicationVersionService();
            
            MemoryWarning = new MemoryWarningService();
            
            LevelEnvironment = new LevelEnvironment();

            AbTestService = new AbTestService();

            RateUsService = new RateUsService(AbTestService);

            BackgroundService = new BackgroundService();

            LevelGraphicService = new LevelGraphicService();

            AnalyticImplementationService = new AppsFlyerAnalyticImplementationService();

            MusicService = new MusicService(LevelEnvironment);

            Time = new TimeService(BackgroundService);
            
            CommonStatisticService = new CommonStatisticsService(LevelEnvironment,
                                                                 AbTestService);

            PlayerStatisticService = new PlayerStatisticService(AbTestService,
                                                                LevelEnvironment,
                                                                BackgroundService);

            LevelOrderService = new LevelOrderService(PlayerStatisticService,
                                                      AbTestService);

            AdsHelperService = new AdsHelperService(AbTestService,
                                                    CommonStatisticService);

            LevelControllerService = new LevelControllerService(LevelEnvironment,
                                                                PlayerStatisticService);

            ShopService = new ShopService(PlayerStatisticService);


            string serverTimeUrl = $"{"https://api.playgendary.com/v1/info/time?build="}" +
                                   $"{TargetSettings.ApplicationIdentifier.Split('.').LastObject()}" +
                                   $"{Application.version}";

            TimeValidator = new TimeValidation(BackgroundService, serverTimeUrl);

            ProposalService = new ProposalService(MusicService,
                                                  CommonStatisticService,
                                                  AbTestService,
                                                  LevelEnvironment,
                                                  RateUsService,
                                                  LevelControllerService,
                                                  ShopService,
                                                  PlayerStatisticService,
                                                  TimeValidator);

            ProposalsLoader = new ProposalsLoader(ProposalService);

            NotificationService = new NotificationService(ProposalService.MonopolyProposeController,
                                                          ProposalService.HitmastersProposeController,
                                                          ProposalService.SeasonEventProposeController,
                                                          ProposalService.MonopolyProposeController,
                                                          ProposalService.HitmastersProposeController,
                                                          ProposalService.SeasonEventProposeController);

            AnalyticTracker = new AnalyticTracker(ProposalService, LevelEnvironment, Time);

            PetsService = new PetsService(ShopService, CommonStatisticService, LevelControllerService);
        }

        #endregion



        #region Private methods

        private void InitializeServices()
        {
            TimeValidator.Initialize();

            ProposalService.MonopolyProposeController.Initialize();
            ProposalService.HitmastersProposeController.Initialize();
            ProposalService.SeasonEventProposeController.Initialize();
            ProposalService.LeagueProposeController.Initialize();

            ProposalService.HappyHoursLeagueProposeController.Initialize();
            ProposalService.HappyHoursSeasonEventProposeController.Initialize();
            
            ProposalService.GetOffer<SubscriptionOffer>().Initialize();
            ProposalService.GetOffer<SubscriptionWidgetOffer>().Initialize();
            ProposalService.GetOffer<GoldenTicketOffer>().Initialize();

            ShopService.IAPs.Initialize();
        }

        #endregion
    }
}
