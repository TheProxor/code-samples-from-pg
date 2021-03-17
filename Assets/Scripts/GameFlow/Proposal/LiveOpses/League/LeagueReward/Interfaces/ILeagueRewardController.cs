using Drawmasters.Proposal.Helpers;
using static Drawmasters.Proposal.LeagueRewardController;


namespace Drawmasters.Proposal.Interfaces
{
    public interface ILeagueRewardController : IInitializable
    {
        RewardData[] GetFinishedEventRewards(string liveOpsId,
                                             LeagueType leagueType,
                                             int boardPosition);

        UiLeagueRewardData GetLeagueRewardPreviewData(string liveOpsId,
                                                      LeagueType leagueType,
                                                      int boardPosition);

        PositionRewardData[] GetLeagueRewardPreviewData(string liveOpsId,
                                                      LeagueType leagueType);

        void ClearSavedRewards(string liveOpsId,
                               LeagueType leagueType);


        bool IsRewardChanged(LeagueType leagueType, int oldPosition, int newPosition);
    }
}