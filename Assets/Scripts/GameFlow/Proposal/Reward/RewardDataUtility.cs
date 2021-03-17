using System;
using System.Collections.Generic;
using System.Linq;


namespace Drawmasters.Proposal
{
    public static class RewardDataUtility 
    {
        #region Methods

        public static void OpenRewards(params RewardData[] data)
        {
            foreach (var d in data)
            {
                if (d != null)
                {
                    d.Open();
                }
                else
                {
                    CustomDebug.Log("Attempt to open NULL reward");
                }
            }
        }


        public static void ApplyRewards(params RewardData[] data)
        {
            foreach (var d in data)
            {
                if (d != null)
                {
                    d.Apply();
                }
                else
                {
                    CustomDebug.Log("Attempt to apply NULL reward");
                }
            }
        }


        public static bool IsLegacyReward(params RewardData[] data)
        {
            if (data == null)
            {
                return false;
            }

            bool wasLegacyRewardFound = Array.Exists(data, e => e is CurrencyReward cr && cr.currencyType == CurrencyType.MansionKeys_Legacy);

            return wasLegacyRewardFound;
        }


        /// <summary>
        /// This method returns priority reward by their weight.
        /// If any skin reward exists in data, then random is perfomed only within skins, otherwise within currency
        /// </summary>
        public static RewardData SelectPriorityAvailableReward(IEnumerable<RewardData> data)
        {
            IEnumerable<RewardData> availableSkinsMixedRewards = SelectAvailableSkinReward(data);
            IEnumerable<RewardData> availableAnotherMixedRewards = data.Where(e => !e.IsSkinReward() && e.IsAvailableForRewardPack);

            IEnumerable<RewardData> rewardForRandom = availableSkinsMixedRewards.Any() ?
                availableSkinsMixedRewards : availableAnotherMixedRewards;

            RewardData result = GetRandomReward(rewardForRandom.ToArray());

            if (result == null)
            {
                CustomDebug.Log($"Selected null data priority" +
                                $" Did you forget to add currency reward in array?");    
            }

            return result;
        }


        public static IEnumerable<RewardData> SelectAvailableSkinReward(IEnumerable<RewardData> data)
        {
            IEnumerable<RewardData> result = data.Where(e => e.IsSkinReward() &&
                                                             e.IsAvailableForRewardPack);
            return result;
        }


        public static bool IsSkinReward(this RewardData rewardData) =>
            rewardData.Type == RewardType.PetSkin ||
            rewardData.Type == RewardType.WeaponSkin ||
            rewardData.Type == RewardType.ShooterSkin;


        public static RewardData GetRandomReward(RewardData[] data)
        {
            data.Shuffle();

            RewardData result = RandomExtentions.RandomWeight(data);

            if (result == null)
            {
                CustomDebug.Log($"Randomized reward is NULL.");
            }

            return result;
        }


        public static RewardData[] RemoveIgnoredCharacterSkinRewards(RewardData[] sourceRewards,
            List<ShooterSkinReward> ignoredRewards) =>
            RemoveIgnoredCharacterSkinRewards(sourceRewards, ignoredRewards.OfType<RewardData>().ToList());

        
        public static RewardData[] RemoveIgnoredCharacterSkinRewards(RewardData[] sourceRewards,
            List<RewardData> ignoredRewards) =>
            RemoveIgnoredRewards<ShooterSkinReward>(sourceRewards,
                ignoredRewards,
                (source, ignored) => source.skinType == ignored.skinType);
        

        public static RewardData[] RemoveIgnoredWeaponSkinRewards(RewardData[] sourceRewards,
            List<WeaponSkinReward> ignoredRewards) =>
            RemoveIgnoredWeaponSkinRewards(sourceRewards, ignoredRewards.OfType<RewardData>().ToList());

        
        public static RewardData[] RemoveIgnoredWeaponSkinRewards(RewardData[] sourceRewards,
            List<RewardData> ignoredRewards) =>
            RemoveIgnoredRewards<WeaponSkinReward>(sourceRewards,
                ignoredRewards,
                (source, ignore) => source.skinType == ignore.skinType);


        private static RewardData[] RemoveIgnoredRewards<T>(RewardData[] sourceRewards,
            List<RewardData> ignoredRewards,
            Func<T, T, bool> match)
        {
            List<RewardData> result = new List<RewardData>(sourceRewards.Length);
            
            List<T> ignored = ignoredRewards.OfType<T>().ToList();

            foreach (var reward in sourceRewards)
            {
                bool isIgnored = false;
                
                if (reward is T ignoredReward)
                {
                    isIgnored = ignored.Contains(i => match.Invoke(ignoredReward, i));
                }

                if (!isIgnored)
                {
                    result.Add(reward);
                }
            }

            return result.ToArray();
        }

        #endregion
    }
}
