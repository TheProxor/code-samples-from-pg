using System;
using System.Collections.Generic;
using System.Linq;
using Drawmasters.Proposal.Helpers;
using Drawmasters.Utils;
using UnityEngine;


namespace Drawmasters.Proposal.Settings
{
    [CreateAssetMenu(fileName = "LeagueRewardSettings", 
        menuName = NamingUtility.MenuItems.ProposalSettings + "LeagueRewardSettings")]    
    public class LeagueRewardSettings : RewardPackSettings
    {
        #region Helper

        [Serializable]
        private class CurrencyRewardSpriteData
        {
            public CurrencyType currencyType = default;
            public Sprite sprite = default;
        }

        [Serializable]
        private class SequenceRewardsData
        {
            public RewardDataInspectorSerialization[] rewardData = default;
        }
        
        #endregion
        
        
        
        #region Fields

        [Header("Настройки финальной награды в зависимости от занятого места включительно")]
        [SerializeField] private List<LeagueFinishRewardData> leagueRewards = default;

        [Header("Настройки промежуточной награды в зависимости от стадии")]
        [SerializeField] private List<LeagueIntermediateRewardData> leagueIntermediateRewards = default;

        [Header("Настройки наград для сундуков:")]
        [Header("Доступные скины персонажа для награды")]
        [SerializeField] private List<ShooterSkinReward> shooterSkinRewards = default;
        
        [Header("Достуные скины оружий для награды")]
        [SerializeField] private List<WeaponSkinReward> weponSkinRewards = default;

        [Header("Доступные питомцы для награды")]
        [SerializeField] private List<PetSkinReward> petSkinRewards = default;

        [Space]
        [Header("Настройки скинов сундуков")]
        [SerializeField] private List<ChestSkinSettings> chestSkinSettings = default;

        [Header("Настройки анимации сундуков")]
        public ChestAnimationSettings chestAnimationSettins = default;
        public float durationClaimReward = default;
        
        [Header("Настройки иконок валютных наград")] 
        [SerializeField] private List<CurrencyRewardSpriteData> currencyRewardSprites = default;

        [Header("Fallback reward for chests")]
        public CurrencyReward fallbackReward = default;

        [Header("Sequence")]
        [SerializeField] private SequenceRewardsData[] sequenceRewardsData = default;
        [SerializeField] private RewardDataInspectorSerialization[] recurringRewardsForTopPlaces = default;

        #endregion



        #region Public methods

        public bool TryGetSequenceRewards(int showIndex, out RewardData[] rewardData)
        {
            rewardData = default;
            bool isDataExists = CommonUtility.IsIndexCorrect(sequenceRewardsData, showIndex);

            if (isDataExists)
            {
                rewardData = sequenceRewardsData[showIndex].rewardData.Select(e => e.RewardData).ToArray();
            }

            return isDataExists;
        }


        public RewardData[] GetReccuringRewards()
        {
            List<RewardData> result = new List<RewardData>();

            RewardData[] availableReward = recurringRewardsForTopPlaces.Select(e => e.RewardData)
                                                                       .Where(e => e.IsAvailableForRewardPack)
                                                                       .ToArray();

            RewardData petReccuring = availableReward.Where(e => e.Type == RewardType.PetSkin).ToArray().RandomObject();
            RewardData shooterReccuring = availableReward.Where(e => e.Type == RewardType.ShooterSkin).ToArray().RandomObject();
            RewardData weaponReccuring = availableReward.Where(e => e.Type == RewardType.WeaponSkin).ToArray().RandomObject();

            result.Add(petReccuring);
            result.Add(shooterReccuring);
            result.Add(weaponReccuring);

            result.RemoveAll(e => e == null);

            return result.ToArray();
        }


        public int GetMinPosition(LeagueType league) =>
            leagueRewards.Where(e => e.leagueType == league).Select(e => e.leaderBoardBeginPosition).Min();


        public int GetMaxPosition(LeagueType league) =>
            leagueRewards.Where(e => e.leagueType == league).Select(e => e.leaderBoardEndPosition).Max();


        public LeagueRewardData FindLeagueFinishRewardData(int position, LeagueType league)
        {
            LeagueFinishRewardData result = leagueRewards.Find(i =>
                i.leagueType == league &&
                position >= i.leaderBoardBeginPosition &&
                position <= i.leaderBoardEndPosition);

            if (result == null)
            {
                CustomDebug.Log($"Can not find league reward. Position: {position}, league: {league}");
            }

            return result;
        }


        public LeagueIntermediateRewardData FindLeagueIntermediateRewardData(float leagueCurrency, int stage, int lastEarnedReward)
        {
            List<LeagueIntermediateRewardData> rewards = leagueIntermediateRewards.FindAll(i =>
                i.stageForClaim == stage &&
                i.leaguePointsForClaim <= leagueCurrency &&
                i.rewardNumber == lastEarnedReward + 1);


            if (rewards == null || rewards.Count == 0)
            {
                return null;
            }

            float max = rewards.Max(x => x.leaguePointsForClaim);
            LeagueIntermediateRewardData result = rewards.Where(x => x.leaguePointsForClaim == max).FirstOrDefault(); 

            return result;
        }


        public bool WasIntermediateRewardDataExsists(int stage, int lastEarnedReward)
        {
            List<LeagueIntermediateRewardData> rewards = leagueIntermediateRewards.FindAll(i =>
               i.stageForClaim == stage &&
               i.rewardNumber == lastEarnedReward + 1);

            return rewards != null && rewards.Count != 0;
        }


        public int IntermediateRewardLastStageIndex()
        {
            List<LeagueIntermediateRewardData> rewards = leagueIntermediateRewards;

            if (rewards == null || rewards.Count == 0)
            {
                CustomDebug.Log("League dont contains Intermediate Rewards");
                return 0;
            }

            return rewards.Max(x => x.stageForClaim);
        }

        public int CalculateIntermediateRewardPointsTopBound(int stage)
        {
            List<LeagueIntermediateRewardData> rewards = leagueIntermediateRewards.FindAll(i =>
               i.stageForClaim == stage);

            if (rewards == null || rewards.Count == 0)
            {
                CustomDebug.Log("League dont contains Intermediate Rewards");
                return 0;
            }

            return rewards.Max(x => x.leaguePointsForClaim);
        }


        public ChestType[] FindIntermediateChestsTypes(int stage)
        {
            List<LeagueIntermediateRewardData> rewards = leagueIntermediateRewards.FindAll(i =>
             i.stageForClaim == stage);

            List<ChestType> result = new List<ChestType>(rewards.Count);

            if (rewards == null || rewards.Count == 0)
            {
                CustomDebug.Log("League dont contains Intermediate Rewards");
                return null;
            }

            foreach (var reward in rewards)
            {
                foreach (var generatedReward in reward.GenerateRewards())
                {
                    if (generatedReward is ChestReward chestReward)
                    {
                        result.Add(chestReward.chestType);
                    }
                }
            }

            return result.ToArray();
        }


        public string FindChestSkin(ChestType chestType)
        {
            ChestSkinSettings skinSettings = chestSkinSettings.Find(i => i.chestType == chestType);

            if (skinSettings == null)
            {
                CustomDebug.Log("Can't find chest skin. Chest type: " + chestType);
            }

            return skinSettings.skinName;
        }


        public Sprite FindCurrencyRewardSprite(CurrencyType currencyType)
        {
            var pair = currencyRewardSprites.Find(i => i.currencyType == currencyType);

            if (pair == null || pair.sprite == null)
            {
                CustomDebug.Log("Cannot find sprite for currency reward. Currency type: " + currencyType);
            }

            return pair.sprite;
        }

        #endregion



        #region Editor
        
        [Sirenix.OdinInspector.Button]
        private void SortCollection()
        {
            leagueRewards.Sort(new LeagueComparer());

            for (int i = 0; i < leagueRewards.Count; i++)
            {
                int previousPosition = 0;
                
                int previousIndex = i - 1;
                if (previousIndex >= 0 && 
                    leagueRewards[i].leagueType == leagueRewards[previousIndex].leagueType)
                {
                    previousPosition = leagueRewards[i - 1].leaderBoardEndPosition + 1;
                }

                leagueRewards[i].leaderBoardBeginPosition = previousPosition;
            }
        }


        [Sirenix.OdinInspector.Button]
        private async void ReloadFromGoogleSheet(int rewardCountForLeague = 3)
        {
            const string DataGoogleSheetID = "1iWaX-yO4X52aTzq7YAiQ01g0SI9hy_O9zKgswNo_Gbg";
            const int DataGID = 955703532;

            await CSVDownloader.ReadDataAsync(DataGoogleSheetID, DataGID, (value) =>
            {
                ParseData(value, rewardCountForLeague);

#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
#endif
            });
        }


        private void ParseData(string value, int rewardCountForLeague = 3)
        {
            leagueRewards.Clear();
            recurringRewardsForTopPlaces = Array.Empty<RewardDataInspectorSerialization>();

            using (var reader = new System.IO.StringReader(value))
            {
                string line = string.Empty;

                for (int i = 0; i < 13; i++)
                {
                    reader.ReadLine(); // skip header
                }

                line = reader.ReadLine();

                while (!string.IsNullOrEmpty(line))
                {
                    string[] values = CSVParseUtility.ParseRowsToArray(line);

                    LeagueType leagueType = default;
                    LeagueFinishRewardData leagueRewardData = default;
              
                    for (int v = 0; v < values.Length; v++)
                    {
                        if (v <= 1)
                        {
                            leagueType = (LeagueType)1;
                            leagueRewardData = GetDefaultData();
                        }
                        else if ((v - 1) % rewardCountForLeague == 0)
                        {
                            var toAdd = leagueRewardData.Clone() as LeagueFinishRewardData;
                            leagueRewards.Add(toAdd);
                            leagueType++;

                            if (leagueType >= (LeagueType)Enum.GetValues(typeof(LeagueType)).Length)
                            {
                                break;
                            }

                            leagueRewardData = GetDefaultData();
                        }

                        leagueRewardData.leaderBoardEndPosition = int.Parse(values[0]) - 1;

                        if (v != 0)
                        {
                            bool shouldAddToReccuringReward = default;
                            LeagueRewardData.LeagueRewards rewards = new LeagueRewardData.LeagueRewards
                            {
                                rewards = new RewardDataInspectorSerialization[] { }
                            };

                            string[] parsedArray = values[v].Contains("/") ? CSVParseUtility.ParseToArray(values[v]) : new string[] { values[v] };

                            foreach (var parsedValue in parsedArray)
                            {
                                RewardDataInspectorSerialization rewardDataInspectorSerialization = default;
                                RewardData targetRewardData = default;

                                string currencyTypeString = new string(parsedValue.Where(e => !char.IsDigit(e) && !char.IsWhiteSpace(e)).ToArray());
                                if (Enum.TryParse(parsedValue, out ShooterSkinType shooterSkinType))
                                {
                                    rewardDataInspectorSerialization = new RewardDataInspectorSerialization();
                                    rewardDataInspectorSerialization.rewardType = RewardType.ShooterSkin;

                                    targetRewardData = new ShooterSkinReward() { skinType = shooterSkinType };

                                    shouldAddToReccuringReward = true;
                                }
                                else if (Enum.TryParse(parsedValue, out WeaponSkinType weaponSkinType))
                                {
                                    rewardDataInspectorSerialization = new RewardDataInspectorSerialization();
                                    rewardDataInspectorSerialization.rewardType = RewardType.WeaponSkin;

                                    targetRewardData = new WeaponSkinReward() { skinType = weaponSkinType };
                                    shouldAddToReccuringReward = true;

                                }
                                else if (Enum.TryParse(parsedValue, out PetSkinType petSkinType))
                                {
                                    rewardDataInspectorSerialization = new RewardDataInspectorSerialization();
                                    rewardDataInspectorSerialization.rewardType = RewardType.PetSkin;

                                    targetRewardData = new PetSkinReward() { skinType = petSkinType };
                                    shouldAddToReccuringReward = true;
                                }
                                else if (Enum.TryParse(parsedValue, out ChestType chestType))
                                {
                                    rewardDataInspectorSerialization = new RewardDataInspectorSerialization();
                                    rewardDataInspectorSerialization.rewardType = RewardType.Chest;

                                    targetRewardData = new ChestReward() { chestType = chestType };
                                }
                                else if (Enum.TryParse(currencyTypeString, out CurrencyType currencyType))
                                {
                                    string currencyCountString = new string(parsedValue.Where(e => char.IsDigit(e)).ToArray());
                                    int.TryParse(currencyCountString, out int currencyCount);

                                    rewardDataInspectorSerialization = new RewardDataInspectorSerialization();
                                    rewardDataInspectorSerialization.rewardType = RewardType.Currency;

                                    targetRewardData = new CurrencyReward() { currencyType = currencyType, value = currencyCount };
                                }
                                else if (!parsedValue.Equals("-"))
                                {
                                    CustomDebug.Log($"Can't create rewardData for string {parsedValue}");
                                }

                                if (rewardDataInspectorSerialization != null &&
                                    targetRewardData != null)
                                {
                                    rewardDataInspectorSerialization.SetRewardData(targetRewardData);
                                    rewards.rewards = rewards.rewards.Add(rewardDataInspectorSerialization);

                                    if (shouldAddToReccuringReward)
                                    {
                                        if (!recurringRewardsForTopPlaces.Contains(e => e.RewardData.IsEqualsReward(targetRewardData)))
                                        {
                                            recurringRewardsForTopPlaces = recurringRewardsForTopPlaces.Add(rewardDataInspectorSerialization);
                                        }

                                        var fallbackReward = new RewardDataInspectorSerialization();
                                        fallbackReward.rewardType = RewardType.Currency;
                                        fallbackReward.SetRewardData(new CurrencyReward()
                                        {
                                            currencyType = CurrencyType.Premium,
                                            value = 100.0f
                                        });

                                        rewards.rewards = rewards.rewards.Add(fallbackReward);
                                    }
                                }
                            }

                            leagueRewardData.mixedRewards = leagueRewardData.mixedRewards.Add(rewards);
                        }


                        LeagueFinishRewardData GetDefaultData()
                        {
                            LeagueFinishRewardData result = new LeagueFinishRewardData();
                            result.leaderBoardBeginPosition = int.Parse(values[0]) - 1;
                            result.leagueRewardType = LeagueRewardType.Pack;
                            result.leagueType = leagueType;
                            result.mixedRewards = new List<LeagueRewardData.LeagueRewards>().ToArray();

                            return result;
                        }
                    }

                    line = reader.ReadLine();
                }
            }

            CustomDebug.Log($"<color=green> Reimorted data for {name} </color>");

            SortCollection();
        }

        #endregion
    }
}