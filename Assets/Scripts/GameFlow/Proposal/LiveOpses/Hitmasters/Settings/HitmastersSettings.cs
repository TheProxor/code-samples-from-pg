using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Drawmasters.Proposal
{
    [CreateAssetMenu(fileName = "HitmastersSettings",
                  menuName = NamingUtility.MenuItems.ProposalSettings + "HitmastersSettings")]
    public class HitmastersSettings : SingleRewardPackSettings
    {
        #region Nested types

        [Serializable]
        private class HitmastersModeRewards
        {
            public GameMode mode = default;
            public ShooterSkinReward[] rewards = default;
        }

        #endregion
        
        
        
        #region Fields

        [Header("Rewards Data")]
        [SerializeField] private HitmastersModeRewards[] rewards = default;

        public float currencyForKilledEnemy = default;

        #endregion



        #region Methods

        public LiveOpsProposeSettings LiveOpsProposeSettings(AbTestData abTestData)
        {
            LiveOpsProposeSettings result = new LiveOpsProposeSettings
            {
                IsAvailable = abTestData.isHitmastersSpinOffAvailable,
                DurationTime = abTestData.hitmastersSpinOffDurationSeconds,
                MinLevelForLiveOps = abTestData.minLevelForHitmastersSpinOff,
                IsReloadTimeUsed = abTestData.hitmastersSpinOffUseReloadTimer,
                ReloadTime = abTestData.hitmastersReloadTime.ReloadSeconds,
                NotificationSecondsBeforeLiveOpsFinish = abTestData.hitmastersNotificationSecondsBeforeLiveOpsFinish,
                AvailabilitySettings = abTestData.hitmastersLiveOpsAvailabilitySettings
            };

            return result;
        }


        public override RewardData[] GetCommonShowRewardPack() => null;
        
        
        public bool IsAllRewardsClaimed()
        {
            bool result = true;

            foreach (var hitmastersReward in rewards)
            {
                ShooterSkinReward[] skinRewards = 
                    Array.FindAll(hitmastersReward.rewards, x => x.IsAvailableForRewardPack);
                
                result &= !skinRewards.Any();
            }
            
            return result;
        }
        
        
        public RewardData GetCommonShowReward(GameMode mode)
        {
            RewardData result = default;
            HitmastersModeRewards arrayRewards = Array.Find(rewards, x => x.mode == mode);

            if (arrayRewards != null)
            {
                RewardData[] skinRewards = Array.FindAll(arrayRewards.rewards, x => x.IsAvailableForRewardPack);
                if (skinRewards.Any())
                {
                    result = RewardDataUtility.GetRandomReward(skinRewards);
                }
            }

            if (result == null)
            {
                result = AvailablePremiumCurrencyRewardData.RandomObject();
            }
            
            return result;
        }
        
        #endregion


        
        #if UNITY_EDITOR
            #region Editor methods

            private void OnValidate()
            {
                List<RewardData> allReward = new List<RewardData>();
                allReward.AddRange(currencyRewards);
                allReward.AddRange(shooterSkinRewards);
                allReward.AddRange(weaponSkinRewards);
                allReward.AddRange(premiumCurrencyRewards);

                foreach (var r in sequenceData)
                {
                    allReward.AddRange(r.AllRewardData);
                }

                foreach (var reward in allReward)
                {
                    reward.receiveType = RewardDataReceiveType.RandomFromPack;
                }
            }

            #endregion
        #endif
    }
}
