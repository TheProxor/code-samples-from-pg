using System;
using Drawmasters.Proposal.Interfaces;


namespace Drawmasters
{
    [Serializable]
    public class HappyHoursSettingsSeasonEvent : IHappyHoursAbSettings
    {
        #region Fields

        public float PlayerPointsMultiplier { get; set; }

        #endregion



        #region IHappyHoursAbSettings

        public bool IsAvailable { get; set; }

        public float DurationSeconds { get; set; }

        public float StartSecondsBeforeLiveOpsFinish { get; set; }

        #endregion
    }
}
