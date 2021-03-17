using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;


namespace Drawmasters.Proposal.Helpers
{
    [Serializable]
    public class LeagueIntermediateRewardData : LeagueRewardData
    {
        #region Fields

        public int rewardNumber = default;
        public int stageForClaim = default;
        public int leaguePointsForClaim = default;

        #endregion



        #region Methods

        public override LeagueRewardData Clone() =>
            (LeagueIntermediateRewardData)MemberwiseClone();

        #endregion
    }
}
