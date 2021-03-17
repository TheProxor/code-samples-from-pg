using Drawmasters.ServiceUtil.Interfaces;


namespace Drawmasters.Pets
{
    public class PetsService : IPetsService
    {
        #region Fields

        private readonly IShopService shopService;
        private readonly ICommonStatisticsService commonStatisticsService;
        private readonly ILevelControllerService levelControllerService;

        #endregion



        #region Class lifecycle

        public PetsService(IShopService _shopService, ICommonStatisticsService _commonStatisticsService, ILevelControllerService _levelControllerService)
        {
            commonStatisticsService = _commonStatisticsService;
            levelControllerService = _levelControllerService;
            shopService = _shopService;

            ChargeController = new PetsChargeController(levelControllerService);
            InvokeController = new PetsInvokeController(ChargeController);
            TutorialController = new PetsTutorialController(ChargeController, shopService);
            RestoreController = new PetsRestoreController(shopService, commonStatisticsService);
        }

        #endregion



        #region IPetsService

        public PetsChargeController ChargeController { get; }

        public PetsInvokeController InvokeController { get; }

        public PetsTutorialController TutorialController { get; }

        public PetsRestoreController RestoreController { get;  }

        #endregion
    }
}
