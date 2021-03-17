using System;
using Drawmasters.ServiceUtil;
using UnityEngine;


namespace Drawmasters.Proposal
{
    public partial class SequenceSingleRewardPackSettings
    {
        #region Nested types

        [Serializable]
        protected class SequenceData
        {
            [SerializeField] private CurrencyReward[] currencyReward = default;
            [SerializeField] private bool useNoMansionCurrencyReward = default;
            [SerializeField] private CurrencyReward[] noMansionCurrencyReward = default;

            [SerializeField] private ShooterSkinReward[] shooterSkinReward = default;
            [SerializeField] private WeaponSkinReward[] weaponSkinReward = default;

            
            public RewardData[] AllRewardData
            {
                get
                {
                    RewardData[] result = Array.Empty<RewardData>();

                    result = result.AddRange(currencyReward);
                    result = result.AddRange(noMansionCurrencyReward);
                    result = result.AddRange(shooterSkinReward);
                    result = result.AddRange(weaponSkinReward);

                    return result;
                }
            }

            public RewardData[] RewardData
            {
                get
                {
                    RewardData[] result = Array.Empty<RewardData>();

                    bool isMansionAvailable = GameServices.Instance.ProposalService.MansionProposeController.IsMechanicAvailable;
                    bool isMansionRewardUnavailable = useNoMansionCurrencyReward && !isMansionAvailable;
                    
                    CurrencyReward[] currencyRewardToAdd = isMansionRewardUnavailable ?
                        noMansionCurrencyReward : 
                        currencyReward;

                    result = result.AddRange(currencyRewardToAdd);
                    result = result.AddRange(shooterSkinReward);
                    result = result.AddRange(weaponSkinReward);

                    return result;
                }
            }
        }

        #endregion
    }
}