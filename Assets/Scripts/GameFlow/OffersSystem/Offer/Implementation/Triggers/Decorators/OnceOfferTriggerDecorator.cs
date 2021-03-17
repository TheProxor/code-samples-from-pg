namespace Drawmasters.OffersSystem
{
    public class OnceOfferTriggerDecorator : BaseOfferTriggerDecorator
    {
        #region Fields

        private BaseOffer offer;

        #endregion



        #region Properties

        public override bool IsActive =>
            base.IsActive &&
            CustomPlayerPrefs.GetInt(offer.ShowCounterKey) <= 1;
          
        #endregion



        #region Class lifecycle

        public OnceOfferTriggerDecorator(BaseOffer _offer, BaseOfferTrigger _trigger) : base(_trigger)
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
