using System;
using Drawmasters.Proposal;
using UnityEngine;
using Modules.General;
using Drawmasters.Levels;


namespace Drawmasters.Pets
{
    public class PetBonusLevelComponent : PetComponent
    {
        #region Fields

        public static Action OnPetSpawn;

        public static Action OnPetBeginTeleportation;
        public static Action OnPetEndTeleportation;


        private readonly PetLevelSettings petLevelSettings;

        private Vector3 defaultPetPosition;

        #endregion



        #region Class lifecycle

        public PetBonusLevelComponent()
        {
            petLevelSettings = IngameData.Settings.pets.levelSettings;
        }

        #endregion



        #region Methods

        public override void Initialize(Pet mainPetValue)
        {
            base.Initialize(mainPetValue);

            RewardCollectComponent.OnRewardDropped += RewardCollectComponent_OnRewardDropped;
        }


        public override void Deinitialize()
        {
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);

            RewardCollectComponent.OnRewardDropped -= RewardCollectComponent_OnRewardDropped;

            base.Deinitialize();
        }


        private void SpawnPet(PhysicalLevelObject referenceBonusLevelObject, PetSkinType petSkinType)
        {
            mainPet.PetSkinChange(petSkinType);

            defaultPetPosition = mainPet.CurrentSkinLink.transform.position;

            mainPet.CurrentSkinLink.transform.position = referenceBonusLevelObject.transform.position;

            OnPetSpawn?.Invoke();

            Scheduler.Instance.CallMethodWithDelay(this, BeginPetTeleportation, petLevelSettings.bonusLevelAppearDelay);
        }


        private void BeginPetTeleportation()
        {
            OnPetBeginTeleportation?.Invoke();

            Scheduler.Instance.UnscheduleMethod(this, BeginPetTeleportation);

            Scheduler.Instance.CallMethodWithDelay(this, EndPetTeleportation, petLevelSettings.bonusLevelTeleportationDelay);
        }


        private void EndPetTeleportation()
        {
            mainPet.CurrentSkinLink.transform.position = defaultPetPosition;

            Scheduler.Instance.UnscheduleMethod(this, EndPetTeleportation);

            OnPetEndTeleportation?.Invoke();
        }

        #endregion



        #region Events handlers

        private void RewardCollectComponent_OnRewardDropped(PhysicalLevelObject physicalLevelObject, BonusLevelObjectData bonusLevelObjectData)
        {
            if (bonusLevelObjectData.rewardType == RewardType.PetSkin)
            {
                SpawnPet(physicalLevelObject, bonusLevelObjectData.petSkinType);
            }
        }

        #endregion
    }
}
