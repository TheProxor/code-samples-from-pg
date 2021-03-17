using Drawmasters;
using Drawmasters.Proposal;


namespace GameFlow.Proposal.League.LeagueReach
{
    public class LeagueReachController
    {
        #region Fields

        private const string ReachedLeagueKey = 
            PrefsKeys.Proposal.League.LeagueReachedLeague;

        private const string WasNewLeagueReachedKey =
            PrefsKeys.Proposal.League.LeageWasNewLeagueReached;

        private const string ReachedLeagueLiveOpsIdKey =
            PrefsKeys.Proposal.League.ReachedLeagueLiveOpsIdKey;

        #endregion



        #region Properties

        public LeagueType ReachedLeague
        {
            get => CustomPlayerPrefs.GetEnumValue<LeagueType>(ReachedLeagueKey);
            private set => CustomPlayerPrefs.SetEnumValue(ReachedLeagueKey, value);
        }

        public bool WasNewLeagueReached
        {
            get => CustomPlayerPrefs.GetBool(WasNewLeagueReachedKey);
            private set => CustomPlayerPrefs.SetBool(WasNewLeagueReachedKey, value);
        }


        public string ReachedLeagueLiveOpsId
        {
            get => CustomPlayerPrefs.GetString(ReachedLeagueLiveOpsIdKey);
            private set => CustomPlayerPrefs.SetString(ReachedLeagueLiveOpsIdKey, value);
        }

        #endregion



        #region Ctor

        public LeagueReachController(LeagueProposeController controller)
        {
            if (!CustomPlayerPrefs.HasKey(ReachedLeagueKey))
            {
                ReachedLeague = LeagueType.Wooden; // TODO: Should be taken from controller, isn't it? from Vladislav.k
                WasNewLeagueReached = false;
                ReachedLeagueLiveOpsId = controller.CurrentLiveOpsId;
            }
        }
        
        #endregion



        #region Public methods

        public bool TryClaimNewReachedLeague(out LeagueType league)
        {
            league = ReachedLeague;

            bool result = WasNewLeagueReached;
            if (result)
            {
                WasNewLeagueReached = false;
            }

            return result;
        }


        public void WriteLeagueLiveOpsId(string liveOpsId) =>
            ReachedLeagueLiveOpsId = liveOpsId;


        public void SetupNewLeague(LeagueType newLeague)
        {
            if (newLeague != ReachedLeague &&
                ReachedLeague.IsLeagueHigher(newLeague))
            {
                WasNewLeagueReached = true;
                
                ReachedLeague = newLeague;
            }
        }

        #endregion
    }
}