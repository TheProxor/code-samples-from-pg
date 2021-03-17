using Drawmasters.Proposal;
using Drawmasters.ServiceUtil;


namespace Drawmasters.OffersSystem
{
    public class AlongWithHappyHoursOfferTriggerDecorator : BaseOfferTriggerDecorator
    {
        #region Fields

        private readonly HappyHoursSeasonEventProposeController controller;
        
        #endregion



        #region Properties

        public override bool IsActive =>
            base.IsActive &&
            controller.IsActive;
            
        #endregion



        #region Class lifecycle

        public AlongWithHappyHoursOfferTriggerDecorator(HappyHoursSeasonEventProposeController _controller, BaseOfferTrigger _trigger) : base(_trigger)
        {
            controller = _controller;
        }

        #endregion



        #region Methods

        public override void Initialize()
        {
            base.Initialize();

            controller.OnStarted += OnStarted;
        }


        public override void Deinitialize()
        {
            controller.OnStarted -= OnStarted;

            base.Deinitialize();
        }

        #endregion



        #region Events handlers

        private void OnStarted()
        {
            if (IsActive)
            {
                InvokeActiveEvent();
            }
        }

        #endregion
    }
}
