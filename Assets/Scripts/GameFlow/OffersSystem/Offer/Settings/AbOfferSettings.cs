using System;

namespace Drawmasters.OffersSystem
{
    [Serializable]
    public class AbOfferSettings
    {
        public bool IsAvailable { get; set; }

        public float DurationTime { get; set; }

        public float CooldownTime { get; set; }
        
        public string Triger { get; set; }

        public string IapID { get; set; }
        
    }
}
