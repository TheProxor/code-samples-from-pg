using System;
using System.Collections.Generic;
using System.Linq;
using Modules.UiKit;
using UnityEngine;


namespace Drawmasters.Proposal
{
    public abstract class SingleRewardPackSettings : SequenceSingleRewardPackSettings
    {
        #region Fields

        [Header("Common params")]
        [SerializeField] protected int rewardCountInPack = default;
        [SerializeField] protected int skinsCountInPack = default;

        [Header("Common rewards")]
        public CurrencyReward[] currencyRewards = default;
        public ShooterSkinReward[] shooterSkinRewards = default;
        public WeaponSkinReward[] weaponSkinRewards = default;
        public CurrencyReward[] premiumCurrencyRewards = default;

        #endregion



        #region Properties

        private RewardData[] AvailableRewardData
        {
            get
            {
                List<RewardData> resultList = new List<RewardData>();

                FillAvailableReward(currencyRewards, ref resultList);
                FillAvailableReward(shooterSkinRewards, ref resultList);
                FillAvailableReward(weaponSkinRewards, ref resultList);
                FillAvailableReward(premiumCurrencyRewards, ref resultList);

                return resultList.ToArray();
            }
        }


        protected RewardData[] AvailableCurrencyRewardData =>
            SelectAvailableRewardData(currencyRewards);


        protected RewardData[] AvailablePremiumCurrencyRewardData =>
            SelectAvailableRewardData(premiumCurrencyRewards);


        protected RewardData[] AvailableShooterSkinsRewardData =>
            SelectAvailableRewardData(shooterSkinRewards);


        protected RewardData[] AvailableWeaponSkinsRewardData =>
            SelectAvailableRewardData(weaponSkinRewards);


        protected RewardData[] AvailableSkinsRewardData
        {
            get
            {
                RewardData[] result = Array.Empty<RewardData>();

                RewardData[] shooterSkinsData = SelectAvailableRewardData(shooterSkinRewards);
                RewardData[] weaponSKinsData = SelectAvailableRewardData(weaponSkinRewards);

                result = result.AddRange(shooterSkinsData);
                result = result.AddRange(weaponSKinsData);

                return result;
            }
        }

        #endregion



        #region Methods

        protected override RewardData[] SelectSequenceRewardData(RewardData[] allCurrentSequenceReward)
        {
            RewardData[] result = new RewardData[rewardCountInPack];

            for (int i = 0; i < rewardCountInPack; i++)
            {
                RewardData[] allowToRandomFromReward = allCurrentSequenceReward.Where(e => !result.Contains(e)).ToArray();
                allowToRandomFromReward = SelectAvailableRewardData(allowToRandomFromReward);

                if (allowToRandomFromReward.Length != 0)
                {
                    RewardData dataToSet = RewardDataUtility.GetRandomReward(allowToRandomFromReward);
                    
                    result[i] = dataToSet;
                }
            }

            for (int i = 0; i < rewardCountInPack; i++)
            {
                if (result[i] == null)
                {
                    RewardData[] rewardDataWithoutResult = AvailableRewardData.Where(e => !result.Contains(e)).ToArray();
                    RewardData data = RewardDataUtility.GetRandomReward(rewardDataWithoutResult);

                    if (data == null)
                    {
                        CustomDebug.Log($"Error when generating reward data in {this}");
                        break;
                    }

                    result[i] = data;
                }
            }

            if (result.Count(e => e != null) != rewardCountInPack)
            {
                CustomDebug.Log($"Error for reward pack generating in {this}");
            }

            return result;
        }

        #endregion



        #region Editor methods

        [Sirenix.OdinInspector.Button]
        private void CheckShooterSkinExists(ShooterSkinType skinType)
        {
            int index = Array.FindIndex(shooterSkinRewards, e => e.skinType == skinType);
            bool isExists = index != -1;

            string colorLog = isExists ? "green" : "red";
            Debug.Log($"<color={colorLog}>Skin type: {isExists}</color>");
        }


        [Sirenix.OdinInspector.Button]
        private void CheckWeaponSkinExists(WeaponSkinType skinType)
        {
            int index = Array.FindIndex(weaponSkinRewards, e => e.skinType == skinType);
            bool isExists = index != -1;

            string colorLog = isExists ? "green" : "red";
            Debug.Log($"<color={colorLog}>Skin type: {isExists}. Index is {index}</color>");
        }

        #endregion
    }
}
