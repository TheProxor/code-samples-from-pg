using Drawmasters.AbTesting;


namespace Drawmasters.Proposal
{
    public class LiveOpsProposeSettings
    {
        public bool IsAvailable { get; set; }

        public float DurationTime { get; set; }

        public float ReloadTime { get; set; }
        
        public int MinLevelForLiveOps { get; set; }

        public bool IsReloadTimeUsed { get; set; }

        public float NotificationSecondsBeforeLiveOpsFinish { get; set; }
        
        public ProposalAvailabilitySettings AvailabilitySettings { get; set; }
    }
}
