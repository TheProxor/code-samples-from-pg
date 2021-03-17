using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Random = System.Random;


namespace Drawmasters.Proposal
{
    public abstract class RewardPackSettings : ScriptableObject
    {
        #region Fields

        private WeaponType availableWeaponType;
        
        private static readonly Random RandomGenerator = new Random(Guid.NewGuid().GetHashCode());

        #endregion



        #region Properties

        protected virtual bool IsRewardDataAvailable(RewardData data)
        {
            bool result = true;

            if (data is WeaponSkinReward weaponSkinReward &&
                availableWeaponType != WeaponType.None)
            {
                WeaponType rewardWeaponType = IngameData.Settings.weaponSkinSettings.GetWeaponType(weaponSkinReward.skinType);
                result &= availableWeaponType == rewardWeaponType;
            }

            return result;
        }

        #endregion



        #region Public methods

        public void SetupWeaponType(WeaponType _availableWeaponType) =>
            availableWeaponType = _availableWeaponType;

        #endregion
        
        
        
        #region Protected methods
        
        protected RewardData[] SelectAvailableRewardData(RewardData[] data)
        {
            List<RewardData> resultList = new List<RewardData>();
            
            FillAvailableReward(data, ref resultList);

            return resultList.ToArray();
        }


        protected void FillAvailableReward(RewardData[] rewards, ref List<RewardData> resultList)
        {
            resultList = resultList ?? new List<RewardData>();
            IEnumerable<RewardData> availableReward = rewards.Where(e => e.IsAvailableForRewardPack && IsRewardDataAvailable(e));
            resultList.AddRange(availableReward);
        }

        #endregion
    }
}
