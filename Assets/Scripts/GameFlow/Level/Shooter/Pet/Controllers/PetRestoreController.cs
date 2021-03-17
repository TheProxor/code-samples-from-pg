using Drawmasters.ServiceUtil.Interfaces;
using Drawmasters.Proposal;

namespace Drawmasters.Pets
{
    public class PetsRestoreController
    {
        #region Fields

        private readonly ICommonStatisticsService commonStatisticsService;
        private readonly IShopService shopService;

        #endregion



        #region Ctor

        public PetsRestoreController(IShopService _shopService, ICommonStatisticsService _commonStatisticsService)
        {
            commonStatisticsService = _commonStatisticsService;
            shopService = _shopService;
        }

        #endregion



        #region Methods

        public void TryRestoreDefaultPet()
        {
            int currentLevelsFinishedCount = commonStatisticsService.GetLevelsFinishedCount(GameMode.Draw);

            PetLevelSettings petLevelSettings = IngameData.Settings.pets.levelSettings;

            PetSkinReward petSkinReward = petLevelSettings.defaultPetSkinReward;

            if (shopService.PetSkins.IsBought(petSkinReward.skinType))
            {
                return;
            }

            if (currentLevelsFinishedCount >= petLevelSettings.levelForUnlockDefaultPet)
            {
                petSkinReward.Open();
                petSkinReward.Apply();
            }
        }

        #endregion
    }
}
