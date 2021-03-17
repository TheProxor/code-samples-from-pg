using System.Collections.Generic;

namespace Drawmasters.ServiceUtil.Interfaces
{
    public interface IAnalyticImplementationService 
    {
        void LogEvent(string eventName, Dictionary<string, string> parameters);
    }
}

