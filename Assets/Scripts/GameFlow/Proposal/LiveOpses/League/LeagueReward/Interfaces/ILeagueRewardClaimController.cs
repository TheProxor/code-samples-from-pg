using System.Collections.Generic;
using static Drawmasters.Proposal.LeagueRewardController;


namespace Drawmasters.Proposal.Interfaces
{
    public interface ILeagueRewardClaimController
    {
        bool IsActive { get; }

        bool CanClaimReward { get; }


        void RecalculateFinishReward();


        bool TryClaimFinishReward(out List<RewardData> data, out List<PositionRewardData> uiLeagueRewardData);


        bool TryClaimIntermediateReward(out List<RewardData> data);
    }
}