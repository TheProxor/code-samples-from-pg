using System.Collections.Generic;


namespace Drawmasters.Proposal.Helpers
{
    public class LeagueComparer : IComparer<LeagueFinishRewardData>
    {
        public int Compare(LeagueFinishRewardData x, LeagueFinishRewardData y)
        {
            int xLeagueIndex = (int) x.leagueType;
            int yLeagueIndex = (int) y.leagueType;

            if (xLeagueIndex > yLeagueIndex)
            {
                return -1;
            }
            else if (xLeagueIndex < yLeagueIndex)
            {
                return 1;
            }
            else
            {
                if (x.leaderBoardEndPosition < y.leaderBoardEndPosition)
                {
                    return -1;
                }
                else if (x.leaderBoardEndPosition > y.leaderBoardEndPosition)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }

        }
    }
}