using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Drawmasters.Proposal
{
    public partial class ForceMeterRewardPackSettings
    {
        #region Nested types

        [Serializable]
        public class SegmentData
        {
            public CurrencyReward[] currencyRewards = default;
            public ShooterSkinReward[] shooterSkinRewards = default;
            public WeaponSkinReward[] weaponSkinRewards = default;
            
            [Tooltip("Делаем выборку из этого массива, если все скины скуплены")]
            public CurrencyReward[] premiumCurrencyRewards = default;


            public RewardData[] AllCommonRewards
            {
                get
                {
                    var availableCurrencyRewards = currencyRewards
                        .Where(e => e.IsAvailableForRewardPack)
                        .Select(i => i as RewardData);
                    
                    List<RewardData> result = new List<RewardData>();
                    
                    result.AddRange(shooterSkinRewards);
                    result.AddRange(weaponSkinRewards);
                    result.AddRange(availableCurrencyRewards);

                    return result.ToArray();
                }
            }
        }

        #endregion   
    }
}