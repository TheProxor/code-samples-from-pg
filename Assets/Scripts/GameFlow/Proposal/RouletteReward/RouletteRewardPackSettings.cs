using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;


namespace Drawmasters.Proposal
{
    [CreateAssetMenu(fileName = "RouletteRewardPackSettings",
                  menuName = NamingUtility.MenuItems.ProposalSettings + "RouletteRewardPackSettings")]
    public class RouletteRewardPackSettings : SingleRewardPackSettings
    {
        #region Nested types

        [Serializable]
        private class ChancesInfo
        {
            [Range(0, 1)] public float[] chances = default;
        }


        #endregion



        #region Fields

        [Tooltip("Награда из премиум листа идет в рандом, если все скины скуплены." +
                 "Количество премиум награды не может быть больше чем это значение")]
        [SerializeField] private int maxPremiumRewardCountInPack = default;

        [Tooltip("Награда из этого листа идет в рандом со скинами и бест прайз генерируется отсюда тоже." +
                 "Актуально для молотков/ключей и т.д.")]
        [SerializeField] private CurrencyReward[] additionalBestPrizeReward = default;

        [Tooltip("Берется последовательно, когда заканчивается - всегда начинает работать commonBestRewardChances")]
        [SerializeField] private ChancesInfo[] sequenceBestRewardChances = default;
        [SerializeField] private ChancesInfo commonBestRewardChances = default;

        #endregion



        #region Methods

        public override RewardData[] GetCommonShowRewardPack()
        {
            RewardData[] sequenceData = GetSequenceData();
            List<RewardData> ignoreShooterSkinRewards =
                sequenceData.Where(e => e.Type == RewardType.ShooterSkin).ToList();
            List<RewardData> ignoreWeaponSkinReward = 
                sequenceData.Where(e => e.Type == RewardType.WeaponSkin).ToList();
            
            List<RewardData> result = new List<RewardData>();

            bool wasAllSkinsClaimed = AvailableSkinsRewardData.All(e => result.Contains(e));

            for (int i = 0; i < rewardCountInPack; i++)
            {
                bool isFullSkinsRewardFilled = result
                    .Count(e => Array.Exists(AvailableSkinsRewardData, d => e == d) || 
                                Array.Exists(additionalBestPrizeReward, d => e == d)) >= skinsCountInPack;
                
                bool isAnySkinRest = AvailableSkinsRewardData.Any(e => !result.Contains(e));

                bool isFullPremiumRewardFilled = result
                    .Count(e => Array.Exists(AvailablePremiumCurrencyRewardData, d => e == d) || 
                                Array.Exists(additionalBestPrizeReward, d => e == d)) >= maxPremiumRewardCountInPack;
               
                bool isAnyPremiumRewardRest = AvailablePremiumCurrencyRewardData.Any(e => !result.Contains(e));

                RewardData[] rewardData = default;

                if (!isFullSkinsRewardFilled && isAnySkinRest)
                {
                    rewardData = AvailableSkinsRewardData;
                    rewardData = rewardData.AddRange<RewardData>(additionalBestPrizeReward);
                }
                else if (wasAllSkinsClaimed && !isFullPremiumRewardFilled && isAnyPremiumRewardRest)
                {
                    rewardData = AvailablePremiumCurrencyRewardData;
                    rewardData = rewardData.AddRange<RewardData>(additionalBestPrizeReward);
                }
                else
                {
                    rewardData = AvailableCurrencyRewardData;
                }

                if (!ignoreShooterSkinRewards.IsNullOrEmpty())
                {
                    rewardData = RewardDataUtility.RemoveIgnoredCharacterSkinRewards(rewardData,
                        ignoreShooterSkinRewards);
                }

                if (!ignoreWeaponSkinReward.IsNullOrEmpty())
                {
                    rewardData = RewardDataUtility.RemoveIgnoredWeaponSkinRewards(rewardData,
                        ignoreWeaponSkinReward);
                }
                
                rewardData = rewardData.ToList().Where(e => !result.Contains(e)).ToArray();
                RewardData data = RewardDataUtility.GetRandomReward(rewardData);

                if (data != null)
                {
                    result.Add(data);
                }
            }

            result.Shuffle();

            return result.ToArray();
        }

        public bool ShouldGiveBestReward(int showIndex, int iterationIndex)
        {
            bool result;

            ChancesInfo chancesInfo = showIndex < sequenceBestRewardChances.Length && showIndex >= 0 ?
                sequenceBestRewardChances[showIndex] : commonBestRewardChances;

            if (iterationIndex < chancesInfo.chances.Length)
            {
                result = chancesInfo.chances[iterationIndex] != 0 && Random.value <= chancesInfo.chances[iterationIndex];
            }
            else
            {
                CustomDebug.Log($"No data found for chances for iterationIndex = {iterationIndex}");
                result = false;
            }

            return result;
        }

        #endregion



        #region Editor methods

        private void OnValidate()
        {
            foreach (var reward in currencyRewards)
            {
                reward.receiveType = RewardDataReceiveType.Default;
                reward.currencyType = CurrencyType.Simple;
            }

            foreach (var reward in shooterSkinRewards)
            {
                reward.receiveType = RewardDataReceiveType.Default;
            }

            foreach (var reward in weaponSkinRewards)
            {
                reward.receiveType = RewardDataReceiveType.Default;
            }

            foreach (var reward in premiumCurrencyRewards)
            {
                reward.receiveType = RewardDataReceiveType.Default;
            }
        }

        #endregion
    }
}
