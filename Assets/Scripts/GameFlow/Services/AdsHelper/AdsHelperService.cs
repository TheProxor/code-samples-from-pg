using Drawmasters.Advertising;
using Drawmasters.ServiceUtil.Interfaces;

namespace Drawmasters.ServiceUtil
{
    public class AdsHelperService : IAdsHelperService
    {
        #region IAdsHelperService

        public AdsLimiter AdsLimiter { get; private set; }

        public LifecycleAdController InactivityAdsController { get; private set; }

        #endregion



        #region Ctor

        public AdsHelperService(IAbTestService abTestService, ICommonStatisticsService commonStatisticsService)
        {
            AdsLimiter = new AdsLimiter(abTestService, commonStatisticsService);
            InactivityAdsController = new LifecycleAdController();
        }

        #endregion
    }
}

