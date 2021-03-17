using System;


namespace Drawmasters.Proposal
{
    [Serializable]
    public class SeasonEventAbSettings
    {
        public int PointsPerLevel { get; set; }
        public SeasonEventReturnType ReturnType { get; set; }

        public int RewardsVariantIndex { get; set; }
        public int PointsPerStepVariantIndex { get; set; }

        public bool LockIfPreviousUnclaimed { get; set; }

        public string AdModule { get; set; }

        public bool GoldenTicketLock { get; set; }
    }
}