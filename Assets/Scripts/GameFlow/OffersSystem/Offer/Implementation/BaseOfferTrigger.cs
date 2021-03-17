using System;


namespace Drawmasters.OffersSystem
{
    public class BaseOfferTrigger : IInitializable, IDeinitializable
    {
        public event Action OnActivated;

        public virtual bool IsActive { get; } = true;

        protected void InvokeActiveEvent() =>
            OnActivated?.Invoke();
        
        public virtual void Initialize() { }

        public virtual void Deinitialize() { }
    }
}