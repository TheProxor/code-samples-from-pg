using CrossPromo;
using Drawmasters.Ua;
using Drawmasters.AbTesting;
using Drawmasters.Interfaces;
using Drawmasters.ServiceUtil.Interfaces;
using Modules.General;
using Modules.MoPub;
using SharedAbTestService = AbTest.AbTestService;
using ISharedAbTestService = Modules.General.Abstraction.IAbTestService;


namespace Drawmasters.ServiceUtil
{
    public class AbTestService : IAbTestService
    {
        #region Fields

        public static IUaAbTestMechanic UaAbTestMechanic { get; } = new CommonMechanicAvailability(PrefsKeys.Dev.DevAbTestHardcodedData);

        #endregion



        #region Properties

        public static bool ShouldUseHardcoadedData =>
            UaAbTestMechanic.WasAvailabilityChanged ?
                UaAbTestMechanic.IsMechanicAvailable : PuzzlemastersDevContent.IsUaBuild;

        #endregion



        #region Ctor

        public AbTestService()
        {
            SharedAbTestService service = Services.GetService<ISharedAbTestService>() as SharedAbTestService;
            CommonData = ShouldUseHardcoadedData ? new AbTestData() : service.GetTestData<AbTestData>();

            MopubData = service.GetTestData<MoPubAbTestKeysData>();
            AdsData = service.GetTestData<IngameAdsAbTestData>();
            CrossPromoData = service.GetTestData<CrossPromoAbTestData>();
        }

        #endregion



        #region IAbTestService

        public AbTestData CommonData { get; }

        public MoPubAbTestKeysData MopubData { get; }

        public IngameAdsAbTestData AdsData { get; }

        public CrossPromoAbTestData CrossPromoData { get; }
        
        #endregion
    }
}
