using System;
using System.Collections.Generic;
using System.Linq;
using Drawmasters.ServiceUtil;
using UnityEngine;


namespace Drawmasters.Proposal
{
    [CreateAssetMenu(fileName = "MonopolySettings",
                  menuName = NamingUtility.MenuItems.ProposalSettings + "MonopolySettings")]
    public class MonopolySettings : SingleRewardPackSettings
    {
        #region Fields

        [Header("Rewards Data")]
        [SerializeField] private CurrencyReward[] cellRewards = default;
        [SerializeField] private CurrencyReward[] cellNoMansionRewards = default;

        [SerializeField] private CurrencyReward[] firstLapsRewards = default;
        [SerializeField] private CurrencyReward[] firstLapsRewardsNoMansion = default;
        [SerializeField] private CurrencyReward[] secondLapsRewards = default;
        [SerializeField] private WeaponSkinReward[] thirdLapsWeaponRewards = default;
        [SerializeField] private ShooterSkinReward[] thirdLapsShooterRewards = default;
        [SerializeField] private CurrencyReward[] thirdLapsCurrencyRewards = default;

        [SerializeField] private int[] countsLapsForReward = default;

        [SerializeField] private RandomExtentions.DropWeight[] rollDropWeight = default;
        
        
        
        [Header("Spin Roulette Common Settings")]

        public CurrencyReward firstStartReward = default;
        public CurrencyReward rewardForAds = default;
        public CurrencyReward rewardForCurrencyBuy = default;

        [Header("Sequence")]
        [Tooltip("Хардкод по запросу гд - при генерации награды со фри спина падает определенная пушка. Данный скин должен быть в пуле наград")]
        [SerializeField] private WeaponSkinType[] firstSequenceWeaponSkinRewards = default;
        [Tooltip("Хардкод по запросу гд - при генерации награды с первого рв спина падает определенная пушка. Данный скин должен быть в пуле наград")]
        [SerializeField] private ShooterSkinType[] firstSequenceShooterSkinRewards = default;

        #endregion



        #region Properties

        public int DescMovementsForLaps =>
            GetCommonShowRewardPack().Length;


        public int[] CountsLapsForReward =>
            countsLapsForReward;

        #endregion



        #region Methods

        public override RewardData[] GetCommonShowRewardPack() =>
            GameServices.Instance.AbTestService.CommonData.isMansionAvailable ? cellRewards : cellNoMansionRewards;
            
        
        public RewardData[] GetRandomLapReward(int eventIndex, RewardData lastReward = null)
        {
            List<RewardData> result = new List<RewardData>(3);

            CurrencyReward[] availableFirstLapsReward = GameServices.Instance.AbTestService.CommonData.isMansionAvailable ? firstLapsRewards : firstLapsRewardsNoMansion;
            RewardData firstReward = RewardDataUtility.GetRandomReward(availableFirstLapsReward);
            result.Add(firstReward);

            CurrencyReward[] availableSecondLapsReward = secondLapsRewards.Where(e => e.IsAvailableForRewardPack).ToArray();
            RewardData secondReward = RewardDataUtility.GetRandomReward(availableSecondLapsReward);
            result.Add(secondReward);

            if (eventIndex > 2)
            {
                List<RewardData> rewards = new List<RewardData>();
                if (lastReward == null)
                {
                    rewards.AddRange(thirdLapsWeaponRewards.Where(x => x.IsAvailableForRewardPack).ToList<RewardData>());   
                    rewards.AddRange(thirdLapsShooterRewards.Where(x => x.IsAvailableForRewardPack).ToList<RewardData>());   
                }
                else if (lastReward is WeaponSkinReward weaponReward)
                {
                    rewards.AddRange(thirdLapsWeaponRewards.Where(
                        x => x.IsAvailableForRewardPack && x.skinType != weaponReward.skinType
                    ).ToList<RewardData>());
                    rewards.AddRange(thirdLapsShooterRewards.Where(x => x.IsAvailableForRewardPack).ToList<RewardData>());

                    if (rewards.Count == 0)
                    {
                        rewards.Add(weaponReward);
                    }
                }
                else if (lastReward is ShooterSkinReward shooterReward)
                {
                    rewards.AddRange(thirdLapsWeaponRewards.Where(x => x.IsAvailableForRewardPack).ToList<RewardData>());   
                    rewards.AddRange(thirdLapsShooterRewards.Where(
                        x => x.IsAvailableForRewardPack && x.skinType != shooterReward.skinType
                    ).ToList<RewardData>());
                    
                    if (rewards.Count == 0)
                    {
                        rewards.Add(shooterReward);
                    }
                }

                if (rewards.Count == 0)
                {
                    rewards = thirdLapsCurrencyRewards.ToList<RewardData>();
                }
                
                RewardData thirdReward = RewardDataUtility.GetRandomReward(rewards.ToArray());
                
                result.Add(thirdReward);
            }
            else
            {
                if (eventIndex <= thirdLapsShooterRewards.Length)
                {
                    result.Add(thirdLapsShooterRewards[eventIndex - 1]);    
                }
                else
                {
                    result.Add(thirdLapsShooterRewards.FirstOrDefault(x => x.IsAvailableForRewardPack));
                }
            }
            return result.ToArray();
        }


        public RewardData GetRandomSpinReward(int showIndex, RewardData[] data, bool isFreeSpin)
        {
            RewardData result = default;

            bool canProposeSequenceWeapon = showIndex < firstSequenceWeaponSkinRewards.Length;
            bool canProposeSequenceShooter = showIndex < firstSequenceShooterSkinRewards.Length;

            if (canProposeSequenceWeapon || canProposeSequenceShooter)
            {
                Func<RewardData, bool> findPredicate = isFreeSpin ?
                    new Func<RewardData, bool>(e => canProposeSequenceWeapon && e is WeaponSkinReward weaponSkinReward && weaponSkinReward.skinType == firstSequenceWeaponSkinRewards[showIndex]) :
                    e => canProposeSequenceShooter && e is ShooterSkinReward shooterSkinReward && shooterSkinReward.skinType == firstSequenceShooterSkinRewards[showIndex];

                result = data.Find(findPredicate);
            }

            result = result ?? RewardDataUtility.GetRandomReward(data);

            return result;
        }


        public int GenerateRollValue()
        {
            int result = RandomExtentions.RandomWeight(rollDropWeight);
            return result;
        }
            


        public LiveOpsProposeSettings LiveOpsProposeSettings(AbTestData abTestData)
        {
            LiveOpsProposeSettings result = new LiveOpsProposeSettings
            {
                IsAvailable = abTestData.isMonopolyAvailable,
                DurationTime = abTestData.monopolyDurationSeconds,
                MinLevelForLiveOps = abTestData.minLevelForMonopoly,
                IsReloadTimeUsed = abTestData.monopolySpinOffUseReloadTimer,
                ReloadTime = abTestData.monopolyReloadTime.ReloadSeconds,
                NotificationSecondsBeforeLiveOpsFinish = abTestData.monopolyNotificationSecondsBeforeLiveOpsFinish,
                AvailabilitySettings = abTestData.monopolyAvailabilitySettings
            };

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
