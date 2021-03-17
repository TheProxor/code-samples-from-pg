using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;


namespace Drawmasters.Proposal.Helpers
{
    [Serializable]
    public abstract class LeagueRewardData
    {
        #region Nested Types

        [Serializable]
        public class LeagueRewards
        {
            public RewardDataInspectorSerialization[] rewards = default;
        }

        #endregion



        #region Fields

        public LeagueRewardType leagueRewardType = default;
        
        [ShowIf("leagueRewardType", LeagueRewardType.Pack)]
        [Header("Массив наград - где можно указывать еще один массив, в котором будет выборка по весам " +
                "(валюта рандомится только если скины получены)")]
        public LeagueRewards[] mixedRewards = default;

        #endregion



        #region Methods

        public abstract LeagueRewardData Clone();


        /// <summary>
        /// Note that mustHaveRewards must be valid and available as a reward
        /// </summary>
        /// <param name="mustHaveRewards"></param>
        /// <returns></returns>
        public RewardData[] GenerateRewards(params RewardData[] mustHaveRewards)
        {
            List<RewardData> result = new List<RewardData>();

            RewardData[] data = GetRandomRewardsData(leagueRewardType, mustHaveRewards);

            if (data == null)
            {
                CustomDebug.Log($"Attempt to add NULL data for league reward pack for leagueRewardType = {leagueRewardType}");
            }
            else
            {
                result.AddRange(data);
            }

            return result.ToArray();
        }


        private RewardData[] GetRandomRewardsData(LeagueRewardType leagueRewardType, params RewardData[] mustHaveRewards)
        {
            RewardData[] result;

            switch (leagueRewardType)
            {
                case LeagueRewardType.Pack:
                    var mixedRewardsResult = new List<RewardData>(mixedRewards.Length);
                    foreach (var mixedReward in mixedRewards)
                    {
                        RewardData[] allMixedRewards = mixedReward.rewards.Select(r => r.RewardData).ToArray();

                        if (allMixedRewards.Length > 0)
                        {
                            RewardData existedMustHaveRewardData = allMixedRewards.Find(e => Array.Exists(mustHaveRewards, m => m.IsEqualsReward(e)));
                            RewardData selectedReward = existedMustHaveRewardData ?? RewardDataUtility.SelectPriorityAvailableReward(allMixedRewards);

                            if (selectedReward != null)
                            {
                                mixedRewardsResult.Add(selectedReward);
                            }
                        }
                    }

                    result = mixedRewardsResult.ToArray();
                    break;

                default:
                    CustomDebug.Log($"Not implemented logic for leagueRewardType = {leagueRewardType}");
                    return default;
            }

            bool isCorrectGeneration = result != null;

            if (!isCorrectGeneration)
            {
                CustomDebug.Log($"Reward data generation in {this} for leagueRewardType = {leagueRewardType} was't correct. " +
                                $"Made currency reward as fallback");
            }

            result = isCorrectGeneration ? result.Where(e => e.IsAvailableForRewardPack).ToArray() : default;

            return result;
        }

        #endregion
    }
}
