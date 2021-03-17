using System;
using System.Collections.Generic;
using UnityEngine;


namespace Drawmasters.Proposal
{
    [Serializable]
    public class ChestSkinsRewardSettings : RewardPackSettings
    {
        [Header("Доступные скины персонажа для сундука")]
        [SerializeField] private List<ShooterSkinReward> shooterSkinRewards = default;

        [Header("Достуные скины оружий для сундука")]
        [SerializeField] private List<WeaponSkinReward> weponSkinRewards = default;

        [Header("Доступные питомцы для сундука")]
        [SerializeField] private List<PetSkinReward> petSkinRewards = default;


        public ShooterSkinReward RandomShooterSkinReward
        {
            get
            {
                RewardData[] availableShooterSkinRewards = SelectAvailableRewardData(shooterSkinRewards.ToArray());

                return RewardDataUtility.GetRandomReward(availableShooterSkinRewards) as ShooterSkinReward;
            }
        }


        public PetSkinReward RandomPetSkinReward
        {
            get
            {
                RewardData[] availablePetSkinRewards = SelectAvailableRewardData(petSkinRewards.ToArray());

                return RewardDataUtility.GetRandomReward(availablePetSkinRewards) as PetSkinReward;
            }
        }


        public WeaponSkinReward RandomWeaponSkinReward
        {
            get
            {
                RewardData[] availableWeaponSkinRewards = SelectAvailableRewardData(weponSkinRewards.ToArray());

                return RewardDataUtility.GetRandomReward(availableWeaponSkinRewards) as WeaponSkinReward;
            }
        }
    }
}
