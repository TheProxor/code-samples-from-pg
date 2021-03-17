using Drawmasters.Proposal;
using Modules.General.InAppPurchase;


namespace Drawmasters.OffersSystem
{
    public class GoldenTicketNotActiveDecorator : BaseOfferTriggerDecorator
    {
        #region Fields

        private readonly SeasonEventProposeController seasonEventController;

        #endregion



        #region Properties

        public override bool IsActive =>
            base.IsActive &&
            !seasonEventController.IsSeasonPassActive;

        #endregion



        #region Class lifecycle

        public GoldenTicketNotActiveDecorator(SeasonEventProposeController _seasonEventController, BaseOfferTrigger _trigger) : base(_trigger)
        {
            seasonEventController = _seasonEventController;
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
