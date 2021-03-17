using Drawmasters.Levels.Inerfaces;
using Drawmasters.ServiceUtil;
using Drawmasters.ServiceUtil.Interfaces;
using Drawmasters.Proposal;
using Drawmasters.Pets;
using System.Collections.Generic;


namespace Drawmasters.Levels
{
    public class BonusLevelPetCollectController : ILevelController, IInitialStateReturn
    {
        #region Fields

        private readonly List<PetSkinType> earnedPetSkins = new List<PetSkinType>();

        private PetLevelSettings petLevelSettings;

        #endregion



        #region Properties

        public bool IsAnyPetCollected =>
            earnedPetSkins.Count > 0;

        public PetSkinType LastCollectedPet =>
            earnedPetSkins.Last();

        private float InitalPetCharge =>
            petLevelSettings.chargePointsOnPetCollect;

        #endregion



        #region Methods

        public void Initialize()
        {
            petLevelSettings = IngameData.Settings.pets.levelSettings;

            earnedPetSkins.Clear();

            RewardCollectComponent.OnRewardDropped += RewardCollectComponent_OnRewardDropped;
            Level.OnLevelStateChanged += Level_OnLevelStateChanged;
            LevelProgressObserver.OnLevelStateChanged += LevelStateObserver_OnLevelStateChanged;
        }


        public void Deinitialize()
        {
            RewardCollectComponent.OnRewardDropped -= RewardCollectComponent_OnRewardDropped;
            Level.OnLevelStateChanged -= Level_OnLevelStateChanged;
            LevelProgressObserver.OnLevelStateChanged -= LevelStateObserver_OnLevelStateChanged;
        }


        public void ReturnToInitialState() =>
            RemoveLevelCollectedPetSkins();


        private void RemoveLevelCollectedPetSkins() =>
            earnedPetSkins.Clear();

        #endregion



        #region Events handlers

        private void Level_OnLevelStateChanged(LevelState state)
        {
            if (state == LevelState.Finished)
            {
                foreach (var petSkin in earnedPetSkins)
                {
                    IShopService shopService = GameServices.Instance.ShopService;
                    shopService.PetSkins.Open(petSkin);

                    GameServices.Instance.PlayerStatisticService.PlayerData.CurrentPetSkin = petSkin;

                    PetsChargeController petsChargeController = GameServices.Instance.PetsService.ChargeController;
                    petsChargeController.ResetCharge();
                    petsChargeController.AddChargePoints(InitalPetCharge);
                    petsChargeController.ApplyChargePoints();
                }
            }
        }


        private void LevelStateObserver_OnLevelStateChanged(LevelResult result)
        {
            if (result == LevelResult.Reload)
            {
                RemoveLevelCollectedPetSkins();
            }
        }


        private void RewardCollectComponent_OnRewardDropped(PhysicalLevelObject from, BonusLevelObjectData bonusLevelObjectData)
        {
            if (bonusLevelObjectData.rewardType != RewardType.PetSkin)
            {
                return;
            }

            PetSkinType type = bonusLevelObjectData.petSkinType;

            earnedPetSkins.Add(type);
        }

        #endregion
    }
}
