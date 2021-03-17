using System;
using System.Collections.Generic;
using Drawmasters.Proposal.Helpers;
using Drawmasters.Proposal.Interfaces;
using Drawmasters.Proposal.Settings;
using GameFlow.Proposal.League;


namespace Drawmasters.Proposal
{
    public class LeagueRewardController : ILeagueRewardController
    {
        #region Helpers

        [Serializable]
        private class Data
        {
            public LeagueRewardHoldData leagueRewardHoldData = default;
            public PositionRewardData[] positionsRewardData = default;

            public Data() { }


            public Data(LeagueRewardHoldData _leagueRewardHoldData, PositionRewardData[] _positionRewardData)
            {
                leagueRewardHoldData = _leagueRewardHoldData;
                positionsRewardData = _positionRewardData;

                foreach (var d in positionsRewardData)
                {
                    string saveKey = GetPositionsRewardSaveKey(leagueRewardHoldData.liveOpsId, d.boardPosition);
                    d.rewardDataSerialization = new RewardDataSerializationArray(saveKey);
                }
            }
        }


        // TODO: it must be private. very hotfix for RC. To vladislav.k
        [Serializable]
        public class PositionRewardData
        {
            public int boardPosition = default;
            public LeagueRewardType leagueRewardType = default;
            public RewardDataSerializationArray rewardDataSerialization = default;
        }

        #endregion



        #region Fields

        private const string SavedRewardDataKey = PrefsKeys.Proposal.League.SavedLeagueRewardHoldDataKey;

        private readonly LiveOpsProposeController liveOpsProposeController;
        private readonly LeagueLeaderBoard leaderBoard;

        private List<Data> loadedLeagueRewardHoldData;

        #endregion



        #region Properties

        private static LeagueRewardSettings Settings =>
            IngameData.Settings.league.leagueRewardSettings;


        private List<Data> SavedLeagueRewardHoldData
        {
            get => CustomPlayerPrefs.GetObjectValue<List<Data>>(SavedRewardDataKey);
            set
            {
                CustomPlayerPrefs.SetObjectValue(SavedRewardDataKey, value);
                RefreshLoadedLeagueRewardHoldData();
            }
        }

        #endregion



        #region Class lifecycle

        public LeagueRewardController(LiveOpsProposeController _liveOpsProposeController, LeagueLeaderBoard _leaderBoard)
        {
            liveOpsProposeController = _liveOpsProposeController;
            leaderBoard = _leaderBoard;

            liveOpsProposeController.OnStarted += LiveOpsProposeController_OnLiveOpsStarted;
        }

        #endregion



        #region ILeagueRewardController

        public void Initialize()
        {
            if (!CustomPlayerPrefs.HasKey(SavedRewardDataKey))
            {
                SavedLeagueRewardHoldData = new List<Data>();

                if (liveOpsProposeController.IsActive)
                {
                    LiveOpsProposeController_OnLiveOpsStarted();
                }
            }

            RefreshLoadedLeagueRewardHoldData();
        }


        public RewardData[] GetFinishedEventRewards(string liveOpsId,
                                                    LeagueType leagueType,
                                                    int boardPosition)
        {
            RewardData[] result = Array.Empty<RewardData>();
            
            PositionRewardData positionRewardData = FindPositionRewardData(liveOpsId, leagueType, boardPosition);

            if (positionRewardData != null)
            {
                result = positionRewardData.rewardDataSerialization.Data;
            }

            return result;
        }


        public UiLeagueRewardData GetLeagueRewardPreviewData(string liveOpsId,
                                                             LeagueType leagueType,
                                                             int boardPosition)
        {
            UiLeagueRewardData result;

            PositionRewardData positionRewardData = FindPositionRewardData(liveOpsId, leagueType, boardPosition);

            if (positionRewardData == null)
            {
                return default;
            }

            RewardData[] rewardData = positionRewardData.rewardDataSerialization.Data;
            result = new UiLeagueRewardData(positionRewardData.leagueRewardType, rewardData);

            return result;
        }


        public PositionRewardData[] GetLeagueRewardPreviewData(string liveOpsId,
                                                               LeagueType leagueType)
        {
            Predicate<Data> findPredicate = e => e.leagueRewardHoldData.league == leagueType &&
                                                 e.leagueRewardHoldData.liveOpsId.Equals(liveOpsId, StringComparison.Ordinal);

            bool isDataExists = loadedLeagueRewardHoldData.Exists(findPredicate);

            // This logic is for old users.
            // Reward logic could be changed, so we re-create reward data and give actual reward for previous leagues
            if (!isDataExists)
            {
                CustomDebug.Log($"New leguew reward data was instantly created in {this}");
                AddLeagueData(liveOpsId, leagueType, liveOpsProposeController.ShowsCount);
            }

            PositionRewardData[] result;

            Data foundData = loadedLeagueRewardHoldData.Find(findPredicate);

            if (foundData != null)
            {
                result = foundData.positionsRewardData;
            }
            else
            {
                result = default;

                CustomDebug.Log($"Can't find data for $\nleagueType = {leagueType}" +
                                                      $"\nliveOpsId = {liveOpsId}");
            }

            return result;
        }


        public void ClearSavedRewards(string liveOpsId, LeagueType leagueType)
        {
            List<Data> savedRewards = SavedLeagueRewardHoldData;

            savedRewards.RemoveAll(e => e.leagueRewardHoldData.league == leagueType &&
                                        e.leagueRewardHoldData.liveOpsId.Equals(liveOpsId, StringComparison.Ordinal));

            SavedLeagueRewardHoldData = savedRewards;
        }

        #endregion



        #region Methods

        private PositionRewardData FindPositionRewardData(string liveOpsId,
                                                          LeagueType leagueType,
                                                          int boardPosition)
        {
            Predicate<Data> findPredicate = e => e.leagueRewardHoldData.league == leagueType &&
                                                 e.leagueRewardHoldData.liveOpsId.Equals(liveOpsId, StringComparison.Ordinal);

            bool isDataExists = loadedLeagueRewardHoldData.Exists(findPredicate);

            // This logic is for old users.
            // Reward logic could be changed, so we re-create reward data and give actual reward for previous leagues
            if (!isDataExists)
            {
                CustomDebug.Log($"New leguew reward data was instantly created in {this}");
                AddLeagueData(liveOpsId, leagueType, liveOpsProposeController.ShowsCount);
            }

            PositionRewardData positionRewardData;

            Data foundData = loadedLeagueRewardHoldData.Find(findPredicate);

            if (foundData != null)
            {
                positionRewardData = Array.Find(foundData.positionsRewardData, e => boardPosition == e.boardPosition);
            }
            else
            {
                positionRewardData = default;

                CustomDebug.Log($"Can't find data for \nboardPosition = {boardPosition}" +
                                                    $"\nleagueType = {leagueType}" +
                                                    $"\nliveOpsId = {liveOpsId}");
            }

            return positionRewardData;
        }


        private void RefreshLoadedLeagueRewardHoldData()
        {
            loadedLeagueRewardHoldData = SavedLeagueRewardHoldData;

            foreach (var leagueData in loadedLeagueRewardHoldData)
            {
                foreach (var d in leagueData.positionsRewardData)
                {
                    string saveKey = GetPositionsRewardSaveKey(leagueData.leagueRewardHoldData.liveOpsId, d.boardPosition);
                    d.rewardDataSerialization = new RewardDataSerializationArray(saveKey);
                }
            }
        }


        private void AddLeagueData(string liveOpsId, LeagueType leagueType, int showCount)
        {
            List<Data> savedRewards = SavedLeagueRewardHoldData;

            Data currentData = GetLeagueRewardHoldData(liveOpsId, leagueType, showCount);
            savedRewards.Add(currentData);

            SavedLeagueRewardHoldData = savedRewards;


            Data GetLeagueRewardHoldData(string _liveOpsId, LeagueType _leagueType, int _showCount)
            {
                LeagueRewardHoldData leagueRewardHoldData = new LeagueRewardHoldData()
                {
                    league = _leagueType,
                    liveOpsId = _liveOpsId,
                };

                bool shouldGenerateBySequence = Settings.TryGetSequenceRewards(_showCount - 1, out RewardData[] sequenceRewardData);
                RewardData[] mustHaveRewards = shouldGenerateBySequence ? sequenceRewardData : Settings.GetReccuringRewards();

                int maxPosition = Settings.GetMaxPosition(leagueRewardHoldData.league);
                List<PositionRewardData> positionsRewardData = new List<PositionRewardData>(maxPosition);

                int minPosition = Settings.GetMinPosition(leagueRewardHoldData.league);
                for (int i = minPosition; i <= maxPosition; i++)
                {
                    LeagueRewardData associatedData = Settings.FindLeagueFinishRewardData(i, leagueRewardHoldData.league);

                    PositionRewardData dataToAdd = new PositionRewardData();

                    dataToAdd.boardPosition = i;
                    dataToAdd.leagueRewardType = associatedData.leagueRewardType;

                    string saveKey = GetPositionsRewardSaveKey(leagueRewardHoldData.liveOpsId, i);
                    dataToAdd.rewardDataSerialization = new RewardDataSerializationArray(saveKey);

                    dataToAdd.rewardDataSerialization.Data = associatedData.GenerateRewards(mustHaveRewards);

                    positionsRewardData.Add(dataToAdd);
                }

                return new Data(leagueRewardHoldData, positionsRewardData.ToArray());
            }
        }


        private static string GetPositionsRewardSaveKey(string liveOpsId, int boardPosition) =>
            string.Concat(PrefsKeys.Proposal.League.PositionsRewardDataSaveKey, liveOpsId, "_", boardPosition);


        public bool IsRewardChanged(LeagueType leagueType, int oldPosition, int newPosition)
        {
            LeagueRewardData oldReward = Settings.FindLeagueFinishRewardData(oldPosition, leagueType);
            LeagueRewardData newReward = Settings.FindLeagueFinishRewardData(newPosition, leagueType);

            if (oldReward.leagueRewardType != newReward.leagueRewardType)
            {
                return true;
            }

            RewardData[] oldRewards = oldReward.GenerateRewards();
            RewardData[] newRewards = newReward.GenerateRewards();

            if (oldRewards.Length != newRewards.Length)
            {
                return true;
            }

            for (int i = 0; i < oldRewards.Length; i++)
            {
                if (oldRewards[i].Type != newRewards[i].Type)
                {
                    return true;
                }
            }
            
            return false;
        }

        #endregion



        #region Events handlers

        private void LiveOpsProposeController_OnLiveOpsStarted() =>
            AddLeagueData(liveOpsProposeController.CurrentLiveOpsId, leaderBoard.LeagueType, liveOpsProposeController.ShowsCount);

        #endregion
    }
}