using System;
using System.Collections.Generic;
using Drawmasters.Levels;


namespace Drawmasters.Pets
{
    public class PetsInvokeController 
    {
        #region Fields

        public event Action<PetSkinType> OnInvokePetForLevel;

        private readonly PetsChargeController chargeController;
        private readonly PetLevelSettings petLevelSettings;

        private readonly List<Pet> createdPetsOnLevel;

        #endregion



        #region Properties

        public bool CanCreatePetForLevel =>
            createdPetsOnLevel.Count < petLevelSettings.maxPetsCountOnLevel;


        public bool WasPetInvoked { get; private set; }

        #endregion



        #region Ctor

        public PetsInvokeController(PetsChargeController _chargeController)
        {
            chargeController = _chargeController;

            petLevelSettings = IngameData.Settings.pets.levelSettings;
            createdPetsOnLevel = new List<Pet>();

            PetSkinComponent.OnPetOnLevelCreated += PetSkinComponent_OnPetOnLevelCreated;
            Level.OnLevelStateChanged += Level_OnLevelStateChanged;
        }


        #endregion



        #region Methods

        public void InvokePetForLevel(PetSkinType petSkinType)
        {
            if (petSkinType == PetSkinType.None)
            {
                CustomDebug.Log($"Attempt to call pet skin type {petSkinType} in {this} that is not allowed");
                return;
            }

            if (!chargeController.IsPetCharged(petSkinType))
            {
                CustomDebug.Log($"Attempt to call pet skin type {petSkinType} in {this} that is not charged");
                return;
            }

            chargeController.ResetCharge();

            WasPetInvoked = true;

            LevelProgressObserver.TriggerPetInvoked(petSkinType);

            OnInvokePetForLevel?.Invoke(petSkinType);
        }

        #endregion



        #region Events handlers

        private void PetSkinComponent_OnPetOnLevelCreated(Pet createdPet) =>
            createdPetsOnLevel.Add(createdPet);


        private void Level_OnLevelStateChanged(LevelState state)
        {
            if (state == LevelState.Finished)
            {
                WasPetInvoked = false;

                createdPetsOnLevel.Clear();
            }
        }

        #endregion
    }
}
