using Drawmasters.Advertising;


namespace Drawmasters.ServiceUtil.Interfaces
{
    public interface IAdsHelperService
    {
        AdsLimiter AdsLimiter { get; }

        LifecycleAdController InactivityAdsController { get; }
    }
}

