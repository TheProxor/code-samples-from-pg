using System;
using System.Collections.Generic;
using UnityEngine;


namespace Drawmasters.Proposal
{
    [CreateAssetMenu(fileName = "LeagueSettings",
                  menuName = NamingUtility.MenuItems.ProposalSettings + "LeagueSettings")]
    public class LeagueSettings : SingleRewardPackSettings
    {
        #region Nested types

        [Serializable]
        public class Data
        {
        }

        #endregion



        #region Fields

        [Header("Rewards Data")]
        public Data[] data = default;

        [Header("Reward Replace Data")]
        [Tooltip("В случае если уже получена непотребляемая награда (скины, пет). То она заменяется одной из нижеперечисленных")]
        public CurrencyReward petsReplaceReward = default;
        public CurrencyReward skinsReplaceReward = default;
        public int CountPositionForNextLeagueAchived = 10;
        public int[] LeaderBoardUniqueProposePositions = default;

        [Header("Leaderboard")]
        public float leaderBoardRefreshPeriod = default;

        #endregion



        #region Methods

        public LiveOpsProposeSettings LiveOpsProposeSettings(AbTestData abTestData)
        {
            LiveOpsProposeSettings result = new LiveOpsProposeSettings
            {
                IsAvailable = abTestData.isLeagueAvailable,
                DurationTime = abTestData.leagueDurationSeconds,
                MinLevelForLiveOps = abTestData.minLevelForLeague,
                IsReloadTimeUsed = abTestData.leagueUseReloadTimer,
                ReloadTime = abTestData.leagueReloadTime.ReloadSeconds,
                NotificationSecondsBeforeLiveOpsFinish = abTestData.leagueNotificationSecondsBeforeLiveOpsFinish,
                AvailabilitySettings = abTestData.leagueAvailabilitySettings
            };

            return result;
        }


        public override RewardData[] GetCommonShowRewardPack() =>
            Array.Empty<RewardData>();

        public Data GetRewardsData(int eventIndex)
        {
            Data result = default;

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
