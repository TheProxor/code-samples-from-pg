using UnityEngine.Events;


namespace Drawmasters.Proposal.Interfaces
{
    public interface ILeagueIntermediateRewardController
    {
        #region Properties

        UnityEvent OnApplyLeaguePoints { get; }


        bool IsActive { get; }


        bool IsNewStageReached { get; }


        bool IsNewRewardReached { get;  }


        int CurrentStageIndex { get; }


        int EarnedRewardsPerStage { get; }


        int LastStageIndex { get; }


        int IntermediateRewardPointsBottomBound { get; }


        int IntermediateRewardPointsTopBound { get; }


        float LeaguePointsCount { get; }


        float ReceivedLeaguePoints { get; }


        ChestType [] ChestsOnCurrentStage { get; }

        #endregion



        #region Methods

        void AddLeaguePoints(float value);


        void ApplyLeaguePoints();


        void ResetLeaguePoints();


        RewardData[] GetIntermediateEventRewards();

        #endregion
    }
}