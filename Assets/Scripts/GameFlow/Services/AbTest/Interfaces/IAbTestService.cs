using Drawmasters.AbTesting;
using Modules.MoPub;

namespace Drawmasters.ServiceUtil.Interfaces
{
    public interface IAbTestService : IAbTestGenericService<AbTestData, MoPubAbTestKeysData, IngameAdsAbTestData>
    {
        
    }
}

