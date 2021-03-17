using Drawmasters.ServiceUtil.Interfaces;
using Modules.General.InAppPurchase;


namespace Drawmasters.OffersSystem
{
    public class SubscriptionWidgetActiveDecorator : BaseOfferTriggerDecorator
    {
        #region Fields

        private readonly IAbTestService abTestService;
        
        #endregion



        #region Properties

        public override bool IsActive =>
            base.IsActive &&
            abTestService.CommonData.isSubscriptionWidgetAvailable;

        #endregion



        #region Class lifecycle

        public SubscriptionWidgetActiveDecorator(IAbTestService _abTestService, BaseOfferTrigger _trigger) : base(_trigger)
        {
            abTestService = _abTestService;
        }

        #endregion



        #region Methods

        public override void Initialize()
        {
            base.Initialize();

            if (IsActive)
            {
                InvokeActiveEvent();
            }
        }

        #endregion
    }
}
