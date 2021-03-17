namespace Drawmasters
{
    public static class AnalyticsKeys
    {
        public const string LiveOpsEventStart = "event_start";
        public const string LiveOpsEventEnd = "event_end";
        public const string LiveOpsEventComplete = "event_complete";
        

        public static class LiveOpsParameters
        {
            public const string LevelNumber = "level_number";
            public const string SessionId = "session_id";
            public const string EventType = "event_type";
            public const string EventId = "event_id";
            public const string PositionNumber = "position_number";
            
            public const string HitmastersLevelIdPrefix = "hitm_";
        }

        
        public const string OfferShow = "offer_show";
        public const string OfferPurchase = "offer_purchase";

        
        public static class OfferParameters
        {
            public const string OfferId = "offer_id";
            public const string EntryPoint = "entry_point";
            public const string IapPrice = "iap_price";
            public const string RvPrice = "rv_price";
        }
    }
}
