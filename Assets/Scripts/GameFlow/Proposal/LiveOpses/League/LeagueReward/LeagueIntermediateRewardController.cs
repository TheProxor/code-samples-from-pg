using Drawmasters.Proposal.Helpers;
using Drawmasters.Proposal.Interfaces;
using Drawmasters.Proposal.Settings;
using Drawmasters.ServiceUtil.Interfaces;
using UnityEngine.Events;


namespace Drawmasters.Proposal
{
    public class LeagueIntermediateRewardController : ILeagueIntermediateRewardController
    {
        #region Fields 

        private readonly IPlayerStatisticService playerStatisticService;

        #endregion



        #region Properties

        public UnityEvent OnApplyLeaguePoints { get; } = new UnityEvent();


        public bool IsActive =>
            CurrentStageIndex <= Settings.IntermediateRewardLastStageIndex();


        public bool IsNewStageReached =>
            !Settings.WasIntermediateRewardDataExsists(CurrentStageIndex, EarnedRewardsPerStage);


        public bool IsNewRewardReached
        {
            get
            {
                LeagueIntermediateRewardData data = Settings.FindLeagueIntermediateRewardData(LeaguePointsCount, CurrentStageIndex, EarnedRewardsPerStage);

                if (data == null)
                {
                    return false;
                }

                float boundForEarn = data.leaguePointsForClaim;

                return LeaguePointsCount >= boundForEarn;
            }
        }


        public int IntermediateRewardPointsBottomBound => 0;


        public int IntermediateRewardPointsTopBound =>
            Settings.CalculateIntermediateRewardPointsTopBound(CurrentStageIndex);


        public float LeaguePointsCount
        {
            get => playerStatisticService.PlayerLiveOpsLeagueData.LeagueIntermediateRewardPoints;
            private set => playerStatisticService.PlayerLiveOpsLeagueData.LeagueIntermediateRewardPoints = value;
        }


        public float ReceivedLeaguePoints
        {
            get;
            private set;
        }


        public int CurrentStageIndex
        {
            get => playerStatisticService.PlayerLiveOpsLeagueData.LeagueIntermediateRewardStage;
            private set => playerStatisticService.PlayerLiveOpsLeagueData.LeagueIntermediateRewardStage = value;
        }


        public int LastStageIndex =>
            Settings.IntermediateRewardLastStageIndex();


        public int EarnedRewardsPerStage
        {
            get => playerStatisticService.PlayerLiveOpsLeagueData.LeagueIntermediateRewardEarned;
            private set => playerStatisticService.PlayerLiveOpsLeagueData.LeagueIntermediateRewardEarned = value;
        }


        public ChestType[] ChestsOnCurrentStage =>
            Settings.FindIntermediateChestsTypes(CurrentStageIndex);


        private static LeagueRewardSettings Settings =>
           IngameData.Settings.league.leagueRewardSettings;

        #endregion



        #region Ctor

        public LeagueIntermediateRewardController(IPlayerStatisticService _playerStatisticService)
        {
            playerStatisticService = _playerStatisticService;
        }

        #endregion



        #region Methods

        public void AddLeaguePoints(float value) =>
            ReceivedLeaguePoints += value;


        public void ApplyLeaguePoints()
        {
            LeaguePointsCount += ReceivedLeaguePoints;
            OnApplyLeaguePoints?.Invoke();
            ReceivedLeaguePoints = 0;

            OnApplyLeaguePoints.RemoveAllListeners();
        }


        public void ResetLeaguePoints()
        {
            LeaguePointsCount = 0;
            ReceivedLeaguePoints = 0;
            CurrentStageIndex = 0;
            EarnedRewardsPerStage = 0;
        }


        public RewardData[] GetIntermediateEventRewards()
        {
            LeagueRewardData data = Settings.FindLeagueIntermediateRewardData(LeaguePointsCount, CurrentStageIndex, EarnedRewardsPerStage);

            RewardData[] result = data.GenerateRewards();

            UpReward();

            return result;
        }


        private void UpReward()
        {
            EarnedRewardsPerStage++;

            if (IsNewStageReached)
            {
                EarnedRewardsPerStage = 0;
                LeaguePointsCount = 0;
                CurrentStageIndex++;
            }
        }

        #endregion
    }
}