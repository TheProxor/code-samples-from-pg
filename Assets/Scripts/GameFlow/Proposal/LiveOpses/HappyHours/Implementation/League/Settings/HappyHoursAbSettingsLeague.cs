using System;
using Drawmasters.Proposal.Interfaces;


namespace Drawmasters
{
    [Serializable]
    public class HappyHoursSettingsLeague : IHappyHoursAbSettings
    {
        #region Fields

        public float PlayerSkullsMultiplier { get; set; }

        public float BotsSkullsMultiplier { get; set; }

        #endregion



        #region IHappyHoursAbSettings

        public bool IsAvailable { get; set; }

        public float DurationSeconds { get; set; }

        public float StartSecondsBeforeLiveOpsFinish { get; set; }

        #endregion
    }
}
