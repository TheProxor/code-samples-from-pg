using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Drawmasters.Proposal
{
    [CreateAssetMenu(fileName = "PremiumShopResultRewardPackSettings",
                  menuName = NamingUtility.MenuItems.ProposalSettings + "PremiumShopResultRewardPackSettings")]
    public class PremiumShopResultRewardPackSettings : SingleRewardPackSettings
    {
        #region Fields

        [Header("Премиальный магазин")]
        [Tooltip("Награда из этого листа будет подменять крайний правый слот ")]
        [SerializeField] private CurrencyReward[] rightPackRewards = default;

        #endregion



        #region Properties
        
        private RewardData[] GetAvailableSkinsReward()
        {
            List<RewardData> result = new List<RewardData>();
            result.AddRange(AvailableShooterSkinsRewardData);
            result.AddRange(AvailableWeaponSkinsRewardData);
            return result.ToArray();
        }

        #endregion



        #region Methods

        public override RewardData[] GetCommonShowRewardPack() =>
            SelectRewardPack();


        private RewardData[] SelectRewardPack()
        {
            RewardData[] sequenceData = GetSequenceData();
            List<RewardData> ignoreShooterSkinRewards =
                sequenceData.Where(e => e.Type == RewardType.ShooterSkin).ToList();
            List<RewardData> ignoreWeaponSkinReward = 
                sequenceData.Where(e => e.Type == RewardType.WeaponSkin).ToList();
            
            List<RewardData> result = new List<RewardData>();

            for (int i = 0; i < rewardCountInPack; i++)
            {
                RewardData[] rewardData = default;

                List<RewardData> skinsAndCurrency = new List<RewardData>();
                skinsAndCurrency.AddRange(GetAvailableSkinsReward());
                skinsAndCurrency.AddRange(AvailableCurrencyRewardData);

                rewardData = skinsAndCurrency.ToArray();

                rewardData = rewardData
                    .Where(e => !result.Contains(e))
                    .ToArray();

                if (rewardData.Length == 0)
                {
                    CustomDebug.Log($"Wrong reward select in {this}");
                    Debug.Break();
                }

                if (!ignoreShooterSkinRewards.IsNullOrEmpty())
                {
                    rewardData = RewardDataUtility.RemoveIgnoredCharacterSkinRewards(rewardData,
                        ignoreShooterSkinRewards);
                }

                if (!ignoreWeaponSkinReward.IsNullOrEmpty())
                {
                    rewardData = RewardDataUtility.RemoveIgnoredWeaponSkinRewards(rewardData,
                        ignoreShooterSkinRewards);
                }

                RewardData data = RewardDataUtility.GetRandomReward(rewardData);

                if (data != null)
                {
                    result.Add(data);
                }
            }

            result = result.OrderBy(e => e.receiveType).ToList();

            if (result.Count > 0 && rightPackRewards.Length > 0)
            {
                result[result.Count - 1] = RewardDataUtility.GetRandomReward(rightPackRewards);
            }

            return result.ToArray();
        }

        #endregion



        #region Editor methods

        private void OnValidate()
        {
            List<RewardData> allReward = new List<RewardData>();
            allReward.AddRange(currencyRewards);
            allReward.AddRange(shooterSkinRewards);
            allReward.AddRange(weaponSkinRewards);

            foreach (var reward in allReward)
            {
                if (reward.receiveType == RewardDataReceiveType.Default ||
                    reward.Type == RewardType.Currency)
                {
                    reward.receiveType = reward.receiveType == RewardDataReceiveType.Default ? RewardDataReceiveType.Currency : reward.receiveType;
                }
            }
        }

        #endregion
    }
}
