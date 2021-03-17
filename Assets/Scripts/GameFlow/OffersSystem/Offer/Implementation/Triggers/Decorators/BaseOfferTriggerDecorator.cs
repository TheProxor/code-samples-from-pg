namespace Drawmasters.OffersSystem
{
    public abstract class BaseOfferTriggerDecorator : BaseOfferTrigger, IInitializable, IDeinitializable
    {
        protected BaseOfferTrigger trigger;

        public override bool IsActive =>
            trigger.IsActive;

        public BaseOfferTriggerDecorator(BaseOfferTrigger _trigger)
        {
            trigger = _trigger;
        }

        public override void Initialize()
        {
            base.Initialize();

            trigger.Initialize();
            trigger.OnActivated += InvokeActiveEvent;
        }


        public override void Deinitialize()
        {
            trigger.Deinitialize();
            trigger.OnActivated -= InvokeActiveEvent;

            base.Deinitialize();
        }
    }
}
