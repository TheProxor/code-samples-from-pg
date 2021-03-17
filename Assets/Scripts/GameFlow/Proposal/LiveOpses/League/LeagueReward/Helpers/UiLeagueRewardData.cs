namespace Drawmasters.Proposal.Helpers
{
    public class UiLeagueRewardData
    {
        #region Properties

        public LeagueRewardType LeagueRewardType { get; }

        public RewardData[] RewardData { get; }
        
        #endregion



        #region Ctor

        public UiLeagueRewardData(LeagueRewardType leagueRewardType,
                                  RewardData[] rewardData)
            : this(leagueRewardType)
        {
            RewardData = rewardData;
        }


        private UiLeagueRewardData(LeagueRewardType leagueRewardType)
        {
            LeagueRewardType = leagueRewardType;
        }

        #endregion
    }
}
