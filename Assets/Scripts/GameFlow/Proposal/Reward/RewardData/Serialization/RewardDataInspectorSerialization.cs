using System;
using Sirenix.OdinInspector;
using UnityEngine;


namespace Drawmasters.Proposal
{
    /// <summary>
    /// Accumulates all possible reward data proposals and used for fast serialization in inspector.
    /// Attempt to reduce odin dependencies. For further reduction - custom editor required.
    /// </summary>
    [Serializable]
    public class RewardDataInspectorSerialization
    {
        public RewardType rewardType = default;

        [SerializeField] [ShowIf("rewardType", Value = RewardType.Currency)] private CurrencyReward currencyReward = default;
        [SerializeField] [ShowIf("rewardType", Value = RewardType.ShooterSkin)] private ShooterSkinReward shooterSkinReward = default;
        [SerializeField] [ShowIf("rewardType", Value = RewardType.WeaponSkin)] private WeaponSkinReward weaponSkinReward = default;
        [SerializeField] [ShowIf("rewardType", Value = RewardType.PetSkin)] private PetSkinReward petSkinReward = default;
        [SerializeField] [ShowIf("rewardType", Value = RewardType.Forcemeter)] private ForcemeterReward forcemeterReward = default;
        [SerializeField] [ShowIf("rewardType", Value = RewardType.Chest)] private ChestReward chestReward = default;
        [SerializeField] [ShowIf("rewardType", Value = RewardType.SpinRoulette)] private SpinRouletteReward spinRouletteReward = default;
        [SerializeField] [ShowIf("rewardType", Value = RewardType.SpinRouletteCash)] private SpinRouletteCashReward spinRouletteCashReward = default;
        [SerializeField] [ShowIf("rewardType", Value = RewardType.SpinRouletteSkin)] private SpinRouletteSkinReward spinRouletteSkinReward = default;
        [SerializeField] [ShowIf("rewardType", Value = RewardType.SpinRouletteWaipon)] private SpinRouletteWaiponReward spinRouletteWaiponReward = default;


        public RewardData RewardData
        {
            get
            {
                RewardData result = default;

                switch (rewardType)
                {
                    case RewardType.Currency:
                        result = currencyReward;
                        break;

                    case RewardType.ShooterSkin:
                        result = shooterSkinReward;
                        break;

                    case RewardType.WeaponSkin:
                        result = weaponSkinReward;
                        break;

                    case RewardType.PetSkin:
                        result = petSkinReward;
                        break;

                    case RewardType.Forcemeter:
                        result = forcemeterReward;
                        break;

                    case RewardType.SpinRoulette:
                        result = spinRouletteReward;
                        break;

                    case RewardType.SpinRouletteCash:
                        result = spinRouletteCashReward;
                        break;

                    case RewardType.SpinRouletteSkin:
                        result = spinRouletteSkinReward;
                        break;

                    case RewardType.SpinRouletteWaipon:
                        result = spinRouletteWaiponReward;
                        break;

                    case RewardType.Chest:
                        result = chestReward;
                        break;

                    default:
                        Debug.LogError($"No data found for rewardType {rewardType} in {this}");
                        break;
                }

                return result;
            }
        }


        public RewardData SetRewardData(RewardData Targetvalue)
        {
            switch (rewardType)
            {
                case RewardType.Currency:
                    currencyReward = Targetvalue as CurrencyReward;
                    break;

                case RewardType.ShooterSkin:
                    shooterSkinReward = Targetvalue as ShooterSkinReward;
                    break;

                case RewardType.WeaponSkin:
                    weaponSkinReward = Targetvalue as WeaponSkinReward;
                    break;

                case RewardType.PetSkin:
                    petSkinReward = Targetvalue as PetSkinReward;
                    break;

                case RewardType.Forcemeter:
                    forcemeterReward = Targetvalue as ForcemeterReward;
                    break;

                case RewardType.SpinRoulette:
                    spinRouletteReward = Targetvalue as SpinRouletteReward;
                    break;

                case RewardType.SpinRouletteCash:
                    spinRouletteCashReward = Targetvalue as SpinRouletteCashReward;
                    break;

                case RewardType.SpinRouletteSkin:
                    spinRouletteSkinReward = Targetvalue as SpinRouletteSkinReward;
                    break;

                case RewardType.SpinRouletteWaipon:
                    spinRouletteWaiponReward = Targetvalue as SpinRouletteWaiponReward;
                    break;

                case RewardType.Chest:
                    chestReward = Targetvalue as ChestReward;
                    break;

                default:
                    Debug.LogError($"No data found for rewardType {rewardType} in {this}");
                    break;
            }

            return Targetvalue;
        }
    }
}
