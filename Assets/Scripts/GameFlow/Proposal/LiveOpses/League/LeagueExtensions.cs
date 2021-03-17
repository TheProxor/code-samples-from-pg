using System;
using Drawmasters.Helpers;

namespace Drawmasters.Proposal
{
    public static class LeagueExtensions
    {
        public static string ToUiMenuHeaderText(this LeagueType leagueType) =>
            string.Concat(leagueType, " league!").ToUpper();


        public static string ToUiLeagueChangeDescriptionKey(this LeagueType leagueType, string keyPrefix)
        {
            string format = "You're in the {0}\nLeague now!";
            return string.Format(format.Replace("\\n", "\n"), leagueType);
        }


        public static bool IsPreviousLeagueAvailable(this LeagueType leagueType)
        {
            object previousLeagueType = leagueType - 1;
            return Enum.IsDefined(typeof(LeagueType), previousLeagueType);
        }


        public static LeagueType GetPreviousLeague(this LeagueType leagueType)
        {
            if (!IsPreviousLeagueAvailable(leagueType))
            {
                CustomDebug.Log("Previous league is not available. Returned default value");
                return LeagueType.None;
            }

            return leagueType - 1;
        }


        public static bool IsNextLeagueAvailable(this LeagueType leagueType)
        {
            object nextLeagueValue = leagueType + 1;
            return Enum.IsDefined(typeof(LeagueType), nextLeagueValue);
        }


        public static LeagueType GetNextLeague(this LeagueType leagueType)
        {
            if (!IsNextLeagueAvailable(leagueType))
            {
                CustomDebug.Log("Next league is not available. Returned current value");
                return leagueType;
            }

            return leagueType + 1;
        }


        public static bool IsLeagueHigher(this LeagueType thisLeague, LeagueType otherLeague) =>
            (int) otherLeague > (int) thisLeague;
    }
}
