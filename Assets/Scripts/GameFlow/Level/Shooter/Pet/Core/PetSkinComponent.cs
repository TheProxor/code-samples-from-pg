using System;
using Drawmasters.Levels;
using Drawmasters.ServiceUtil;
using Drawmasters.Statistics.Data;


namespace Drawmasters.Pets
{
    public class PetSkinComponent : PetComponent
    {
        #region Fields

        public static Action<Pet> OnPetOnLevelCreated;

        private readonly PlayerData playerData;
        private readonly IPetsService petsService;

        #endregion



        #region Class lifecycle

        public PetSkinComponent()
        {
            playerData = GameServices.Instance.PlayerStatisticService.PlayerData;
            petsService = GameServices.Instance.PetsService;
        }

        #endregion



        #region Methods

        public override void Initialize(Pet _pet)
        {
            base.Initialize(_pet);

            Level.OnLevelStateChanged += Level_OnLevelStateChanged;
            mainPet.OnPetSkinChange += RefreshSkin;
        }


        public override void Deinitialize()
        {
            Level.OnLevelStateChanged -= Level_OnLevelStateChanged;
            mainPet.OnPetSkinChange -= RefreshSkin;

            if (mainPet != null && mainPet.IsPetExists && mainPet.CurrentSkinLink != null)
            {
                mainPet.CurrentSkinLink.Deinitialize();
            }

            base.Deinitialize();
        }


        private void RefreshSkin(PetSkinType petSkinType)
        {
            #warning Is that possible? Yurii.S
            if (mainPet == null)
            {
                return;
            }

            ShooterColorType colorType = mainPet.ColorType;

            if (mainPet != null && mainPet.IsPetExists && mainPet.CurrentSkinLink != null)
            {
                mainPet.CurrentSkinLink.Deinitialize();
            }

            mainPet.RefreshSkin(petSkinType, () =>
            {
                if (!mainPet.IsPetExists)
                {
                    return;
                }

                mainPet.CurrentSkinLink.Initialize(petSkinType, colorType);
            });
        }

        #endregion



        #region Events handlers

        private void Level_OnLevelStateChanged(LevelState state)
        {
            bool isSceneMode = GameServices.Instance.LevelEnvironment.Context.IsSceneMode;
            bool canCreatePet = state == LevelState.Initialized &&
                                (isSceneMode || petsService.InvokeController.CanCreatePetForLevel);
            if (canCreatePet)
            {
                RefreshSkin(playerData.CurrentPetSkin);

                if (!isSceneMode)
                {
                    OnPetOnLevelCreated?.Invoke(mainPet);
                }
            }
        }

        #endregion
    }
}
