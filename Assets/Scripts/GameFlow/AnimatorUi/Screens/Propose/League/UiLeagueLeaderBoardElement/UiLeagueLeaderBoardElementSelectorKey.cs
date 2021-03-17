using System;
using Sirenix.OdinInspector;


namespace Drawmasters.Ui
{
    [Serializable]
    public class UiLeagueLeaderBoardElementSelectorKey
    {
        public LeaderBordItemType type = default;
        public bool shouldCheckNextLeagueAchived = default;

        [ShowIf("shouldCheckNextLeagueAchived")]
        public bool isNextLeagueAchived = default;


        public UiLeagueLeaderBoardElementSelectorKey(LeaderBordItemType _type, bool _isNextLeagueAchived)
        {
            type = _type;
            isNextLeagueAchived = _isNextLeagueAchived;
        }


        public bool IsEquals(UiLeagueLeaderBoardElementSelectorKey obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            bool isNextLeagueEquals = !shouldCheckNextLeagueAchived || isNextLeagueAchived == obj.isNextLeagueAchived;

            bool result = type == obj.type &&
                          isNextLeagueEquals;

            return result;
        }
    }
}
