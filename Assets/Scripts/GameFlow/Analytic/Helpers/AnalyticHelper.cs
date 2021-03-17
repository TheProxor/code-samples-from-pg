using System;
using System.Collections.Generic;
using Drawmasters.ServiceUtil;
using Modules.Analytics;
using UnityEngine;


namespace Drawmasters.Analytic
{
    public static class AnalyticHelper
    {
        #region Public methods

        public static void SendLiveOpsCompleteEvent(string liveOpsName, string eventId) =>
            SendLiveOpsEvent(AnalyticsKeys.LiveOpsEventComplete, liveOpsName, eventId, string.Empty);


        public static void SendLiveOpsStartEvent(string liveOpsName, string eventId) =>
            SendLiveOpsEvent(AnalyticsKeys.LiveOpsEventStart, liveOpsName, eventId, string.Empty);


        public static void SendLiveOpsFinishEvent(string liveOpsName, string eventId, string positionNumber) =>
            SendLiveOpsEvent(AnalyticsKeys.LiveOpsEventEnd, liveOpsName, eventId, positionNumber);


        private static void SendLiveOpsEvent(string eventType, string liveOpsName, string eventId, string positionNumber)
        {
            int levelNumber = GameServices.Instance.CommonStatisticService.LevelsFinishedCount;
            float sessionId = Time.time;

            Dictionary<string, string> parameters = new Dictionary<string, string>()
            {
                {AnalyticsKeys.LiveOpsParameters.LevelNumber, Convert.ToString(levelNumber)},
                {AnalyticsKeys.LiveOpsParameters.SessionId, sessionId.ToString("F0")},
                {AnalyticsKeys.LiveOpsParameters.EventType, liveOpsName},
                {AnalyticsKeys.LiveOpsParameters.EventId, eventId}
            };
            
            if (!string.IsNullOrEmpty(positionNumber))
            {
                parameters.Add(AnalyticsKeys.LiveOpsParameters.PositionNumber, positionNumber);
            }
            
            AnalyticsManager.Instance.SendEvent(eventType, parameters);
        }

        
        public static void SendOfferShow(string offerId, string entryPoint)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>()
            {
                {AnalyticsKeys.OfferParameters.OfferId, offerId},
                {AnalyticsKeys.OfferParameters.EntryPoint, entryPoint},
            };
            
            AnalyticsManager.Instance.SendEvent(AnalyticsKeys.OfferShow, parameters);
        }

        
        public static void SendOfferPurchase(string offerId, string entryPoint, float iapPrice, int rvPrice)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>()
            {
                {AnalyticsKeys.OfferParameters.OfferId, offerId},
                {AnalyticsKeys.OfferParameters.EntryPoint, entryPoint},
                {AnalyticsKeys.OfferParameters.IapPrice, iapPrice.ToString("F2")},
                {AnalyticsKeys.OfferParameters.RvPrice, rvPrice < 0 ? string.Empty : Convert.ToString(rvPrice)},
            };
            
            AnalyticsManager.Instance.SendEvent(AnalyticsKeys.OfferPurchase, parameters);
        }
        
        #endregion
    }
}