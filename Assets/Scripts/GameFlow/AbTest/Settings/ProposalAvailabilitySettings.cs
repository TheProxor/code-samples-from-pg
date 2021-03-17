using System;


namespace Drawmasters.AbTesting
{
    [Serializable]
    public class ProposalAvailabilitySettings
    {
        public int triggerDay = default;
        public float cooldownSeconds = default;
        public int cooldownLevels = default;
    }
}