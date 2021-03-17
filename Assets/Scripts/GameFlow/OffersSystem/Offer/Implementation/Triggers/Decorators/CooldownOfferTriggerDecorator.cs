namespace Drawmasters.OffersSystem
{
    public class CooldownOfferTriggerDecorator : BaseOfferTriggerDecorator
    {
        #region Fields

        private readonly BaseOffer offer;

        #endregion



        #region Properties

        public override bool IsActive =>
            base.IsActive &&
            (offer.MutexObject == null || offer.MutexObject == offer);
          
        #endregion



        #region Class lifecycle

        public CooldownOfferTriggerDecorator(BaseOffer _offer, BaseOfferTrigger _trigger) : base(_trigger)
        {
            offer = _offer;
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
