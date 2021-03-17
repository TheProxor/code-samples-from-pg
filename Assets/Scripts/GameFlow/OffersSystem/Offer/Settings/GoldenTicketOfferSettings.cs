using System;

namespace Drawmasters.OffersSystem
{
    [Serializable]
    public class GoldenTicketOfferSettings : AbOfferSettings
    {
        public int minSeasonEventStepClaimed = default;
        public int minTimeForStart = default;
    }
}