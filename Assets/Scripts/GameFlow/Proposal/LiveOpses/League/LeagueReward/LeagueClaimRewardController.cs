using System;
using System.Collections.Generic;
using Drawmasters.Proposal.Helpers;
using Drawmasters.Proposal.Interfaces;
using Drawmasters.ServiceUtil;
using GameFlow.Proposal.League;
using static Drawmasters.Proposal.LeagueRewardController;

namespace Drawmasters.Proposal
{
    public class LeagueClaimRewardController : ILeagueRewardClaimController
    {
        #region Fields

        private const string SavedRewardDataKey = PrefsKeys.Proposal.League.LeagueSavedRewardDataInfo;

        private readonly LiveOpsProposeController liveOpsProposeController;
        private readonly ILeagueRewardController leagueRewardController;
        private readonly ILeagueIntermediateRewardController leagueIntermediateRewardController;

        #endregion



        #region Properties

        public bool IsActive =>
                liveOpsProposeController.IsMechanicAvailable &&
                liveOpsProposeController.IsEnoughLevelsFinished &&
                liveOpsProposeController.WasFirstLiveOpsStarted;


        public bool CanClaimReward =>
            CanClaimFinishReward || CanClaimIntermediateReward;


        public bool CanClaimFinishReward
        {
            get
            {
                bool canClaim = default;

                List<LeagueRewardHoldData> savedRewards = SavedRewardData;
                foreach (var data in savedRewards)
                {
                    bool currentReward = data.liveOpsId == liveOpsProposeController.CurrentLiveOpsId &&
                                         liveOpsProposeController.IsActive;

                    canClaim = !currentReward;
                    if (canClaim)
                    {
                        break;
                    }
                }

                return canClaim;
            }
        }

        public bool CanClaimIntermediateReward =>
            leagueIntermediateRewardController.IsNewRewardReached;


        private List<LeagueRewardHoldData> SavedRewardData
        {
            get => CustomPlayerPrefs.GetObjectValue<List<LeagueRewardHoldData>>(SavedRewardDataKey);
            set => CustomPlayerPrefs.SetObjectValue(SavedRewardDataKey, value);
        }


        private LeagueRewardHoldData CurrentRewardDataToHold
        {
            get
            {
                LeagueLeaderBoard leaderBoard = GameServices.Instance.ProposalService.LeagueProposeController.LeaderBoard;

                return new LeagueRewardHoldData()
                {
                    league = leaderBoard.LeagueType,
                    liveOpsId = liveOpsProposeController.CurrentLiveOpsId
                };
            }
        }

        #endregion



        #region Ctor

        public LeagueClaimRewardController(LiveOpsProposeController _liveOpsProposeController,
                                           ILeagueRewardController _leagueRewardController,
                                           ILeagueIntermediateRewardController _leagueIntermediateRewardController)
        {
            liveOpsProposeController = _liveOpsProposeController;
            liveOpsProposeController.OnStarted += LiveOpsProposeController_OnLiveOpsStarted;

            leagueRewardController = _leagueRewardController;
            leagueIntermediateRewardController = _leagueIntermediateRewardController;

            if (!CustomPlayerPrefs.HasKey(SavedRewardDataKey))
            {
                SavedRewardData = new List<LeagueRewardHoldData>();
            }
        }

        #endregion



        #region ILeagueRewardClaimController

        public void RecalculateFinishReward()
        {
            LeagueProposeController controller = GameServices.Instance.ProposalService.LeagueProposeController;

            List<LeagueRewardHoldData> holdedData = SavedRewardData;
            LeagueRewardHoldData oldData = default;

            foreach (var i in holdedData)
            {
                bool needRegenerateReward = i.league != controller.LeaderBoard.LeagueType ||
                                            controller.LeaderBoard.CurrentPlayerPosition != controller.LeaderBoard.PreviousLeaderBoardPosition;

                needRegenerateReward &= i.liveOpsId == liveOpsProposeController.CurrentLiveOpsId;

                if (needRegenerateReward)
                {
                    oldData = i;
                    break;
                }
            }

            if (oldData != null)
            {
                holdedData.Remove(oldData);
                holdedData.Add(CurrentRewardDataToHold);

                SavedRewardData = holdedData;
            }
        }


        public bool TryClaimFinishReward(out List<RewardData> data, out List<PositionRewardData> uiLeagueRewardData)
        {
            bool result = false;
            
            List<RewardData> rewards = new List<RewardData>();

            LeagueProposeController controller = GameServices.Instance.ProposalService.LeagueProposeController;

            data = default;
            uiLeagueRewardData = default;

            if (!IsActive || !CanClaimFinishReward)
            {
                return result;
            }

            var savedRewardData = SavedRewardData;

            if (!savedRewardData.IsNullOrEmpty())
            {
                LeagueRewardHoldData savedData = savedRewardData.Find(i => i.liveOpsId != liveOpsProposeController.CurrentLiveOpsId ||
                                                                           !liveOpsProposeController.IsActive);

                rewards.AddRange(leagueRewardController.GetFinishedEventRewards(savedData.liveOpsId,
                                                                                      savedData.league,
                                                                                      controller.LeaderBoard.CurrentPlayerPosition));

                uiLeagueRewardData = new List<PositionRewardData>(leagueRewardController.GetLeagueRewardPreviewData(savedData.liveOpsId, savedData.league));
                leagueRewardController.ClearSavedRewards(savedData.liveOpsId, savedData.league);

                savedRewardData.Remove(savedData);
                SavedRewardData = savedRewardData;
                result = savedData != null;
            }
            else
            {
                CustomDebug.Log("Saved reward data is NULL");
            }

            data = rewards;

            return result;
        }


        public bool TryClaimIntermediateReward(out List<RewardData> data)
        {
            List<RewardData> rewards = new List<RewardData>();

            LeagueProposeController controller = GameServices.Instance.ProposalService.LeagueProposeController;

            data = default;

            if (!IsActive || !CanClaimIntermediateReward)
            {
                return false;
            }

            rewards.AddRange(leagueIntermediateRewardController.GetIntermediateEventRewards());

            data = rewards;

            return data != null && data.Count > 0;
        }

        #endregion



        #region Events handlers

        private void LiveOpsProposeController_OnLiveOpsStarted()
        {
            List<LeagueRewardHoldData> rewards = SavedRewardData;

            rewards.Add(CurrentRewardDataToHold);

            SavedRewardData = rewards;
        }

        #endregion
    }
}
