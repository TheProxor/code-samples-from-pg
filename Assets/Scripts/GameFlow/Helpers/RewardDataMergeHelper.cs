using System;
using System.Collections.Generic;
using Drawmasters.Proposal;

namespace Drawmasters.Helpers
{

    public class RewardDataMergeHelper : IDeinitializable
    {
        private readonly Dictionary<CurrencyType, CurrencyReward> mergedRewardData =
                new Dictionary<CurrencyType, CurrencyReward>();


        public void Deinitialize() =>
            mergedRewardData.Clear();


        public void ApplyMergedReward(Action<CurrencyReward> callback)
        {
            foreach (var reward in mergedRewardData.Values)
            {
                callback?.Invoke(reward);
            }

            mergedRewardData.Clear();
        }


        public bool TryMergeReward(RewardData rewardData)
        {
            bool result = false;

            if (rewardData is CurrencyReward)
            {
                CurrencyReward currencyReward = rewardData.Clone() as CurrencyReward;

                CurrencyType currencyType = currencyReward.currencyType;

                if (!mergedRewardData.ContainsKey(currencyType))
                {
                    mergedRewardData.Add(currencyType, currencyReward);
                }
                else
                {
                    mergedRewardData[currencyType].value += currencyReward.value;
                }

                result = true;
            }

            return result;
        }
    }

}