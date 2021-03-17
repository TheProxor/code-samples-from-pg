using System.Collections.Generic;
using Modules.AppsFlyer;


namespace Drawmasters.ServiceUtil.Interfaces
{
    public class AppsFlyerAnalyticImplementationService : IAnalyticImplementationService
    {
        #region IAnalyticImplementationService

        public void LogEvent(string eventName, Dictionary<string, string> parameters)
        {
            LLAppsFlyerManager.LogRichEvent(eventName, parameters);
        }

        #endregion
    }
}

