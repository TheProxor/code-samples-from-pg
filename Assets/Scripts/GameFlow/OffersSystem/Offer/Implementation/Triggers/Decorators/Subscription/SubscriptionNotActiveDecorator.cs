using Modules.General.InAppPurchase;


namespace Drawmasters.OffersSystem
{
    public class SubscriptionNotActiveDecorator : BaseOfferTriggerDecorator
    {
        #region Fields

        private readonly StoreManager storeManager;

        #endregion



        #region Properties

        public override bool IsActive =>
            base.IsActive &&
            !storeManager.HasAnyActiveSubscription;

        #endregion



        #region Class lifecycle

        public SubscriptionNotActiveDecorator(BaseOfferTrigger _trigger) : base(_trigger)
        {
            storeManager = StoreManager.Instance;
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
