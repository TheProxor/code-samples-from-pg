using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;


namespace Drawmasters.Proposal.Helpers
{
    [Serializable]
    public class LeagueFinishRewardData : LeagueRewardData
    {
        #region Fields

        public LeagueType leagueType = default;

        [DisableInPrefabs]
        public int leaderBoardBeginPosition = default;
        public int leaderBoardEndPosition = default;

        #endregion


        #region Methods

        public override LeagueRewardData Clone() =>
            (LeagueFinishRewardData)MemberwiseClone();

        #endregion
    }
}
