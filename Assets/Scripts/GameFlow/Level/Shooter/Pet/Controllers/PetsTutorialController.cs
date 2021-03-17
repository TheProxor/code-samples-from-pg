using System.Collections.Generic;
using Drawmasters.Levels;
using Drawmasters.ServiceUtil;
using Drawmasters.ServiceUtil.Interfaces;
using Drawmasters.Tutorial;


namespace Drawmasters.Pets
{
    public class PetsTutorialController
    {
        #region Fields

        private readonly PetsChargeController petsChargeController;
        private readonly IShopService shopService;
        private readonly PetLevelSettings petLevelSettings;

        #endregion



        #region Properties

        public bool ShouldReckonPetCharged =>
            NextAllowedTutorialType == TutorialType.PetChargePoints;


        public TutorialType NextAllowedTutorialType
        {
            get
            {
                TutorialType result = default;

                bool allowPetCharge = shopService.PetSkins.BoughtItems.Count > 0 &&
                                      shopService.PetSkins.BoughtItems.Exists(e => e != PetSkinType.None && !petsChargeController.IsPetCharged(e)) &&
                                      !TutorialManager.WasTutorialCompleted(TutorialType.PetChargePoints);
                if (allowPetCharge)
                {
                    result = TutorialType.PetChargePoints;
                    return result;
                }

                bool allowPetInvoke = shopService.PetSkins.BoughtItems.Count > 0 &&
                                      shopService.PetSkins.BoughtItems.Exists(e => e != PetSkinType.None && petsChargeController.IsPetCharged(e)) &&
                                      !TutorialManager.WasTutorialCompleted(TutorialType.PetInvoke);
                if (allowPetInvoke)
                {
                    result = TutorialType.PetInvoke;
                    return result;
                }

                return result;
            }
        }

        #endregion



        #region Ctor

        public PetsTutorialController(PetsChargeController _petsChargeController, IShopService _shopService)
        {
            petsChargeController = _petsChargeController;
            shopService = _shopService;
            petLevelSettings = IngameData.Settings.pets.levelSettings;

            Level.OnLevelStateChanged += Level_OnLevelStateChanged;
        }


        #endregion



        #region Events handlers

        private void Level_OnLevelStateChanged(LevelState state)
        {
            if (state == LevelState.Initialized)
            {
                TutorialType nextAllowedTutorialType = NextAllowedTutorialType;
                if (nextAllowedTutorialType != TutorialType.None)
                {
                    PetSkinType tutorialForceApplySkin;
                    List<PetSkinType> boughtItems = GameServices.Instance.ShopService.PetSkins.BoughtItems;

                    if (nextAllowedTutorialType == TutorialType.PetChargePoints)
                    {
                        PetSkinType[] unchargedPets = boughtItems.FindAll(e => e != PetSkinType.None && !petsChargeController.IsPetCharged(e)).ToArray();
                        tutorialForceApplySkin = unchargedPets.Find(e => e == petLevelSettings.defaultPetSkinReward.skinType);
                        tutorialForceApplySkin = tutorialForceApplySkin == PetSkinType.None ? unchargedPets.FirstObject() : tutorialForceApplySkin;
                    }
                    else if (nextAllowedTutorialType == TutorialType.PetInvoke)
                    {
                        PetSkinType[] chargedPets = boughtItems.FindAll(e => e != PetSkinType.None && petsChargeController.IsPetCharged(e)).ToArray();
                        tutorialForceApplySkin = chargedPets.Find(e => e == petLevelSettings.defaultPetSkinReward.skinType);
                        tutorialForceApplySkin = tutorialForceApplySkin == PetSkinType.None ? chargedPets.FirstObject() : tutorialForceApplySkin;
                    }
                    else
                    {
                        CustomDebug.Log($"Not implemented logic for NextAllowedTutorialType = {nextAllowedTutorialType}");
                        tutorialForceApplySkin = default;
                    }

                    GameServices.Instance.PlayerStatisticService.PlayerData.CurrentPetSkin = tutorialForceApplySkin;
                }
            }
        }
        #endregion
    }
}
