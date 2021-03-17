using System;
using System.Collections.Generic;


namespace Core
{
    public class FilteredEvent<TFilter> : IEvent
    {
        private readonly Dictionary<TFilter, Action> actions = new Dictionary<TFilter, Action>();
        private readonly Queue<TFilter> filters = new Queue<TFilter>();
        

        public void Notify()
        {
            if (!actions.TryGetValue(filters.Dequeue(), out Action action)) return;

            action?.Invoke();
        }



        public virtual void Subscribe(TFilter filter, Action action)
        {
            actions[filter] = actions.TryGetValue(filter, out Action existsAction) ? existsAction + action : action;
        }


        public virtual void Unsubscribe(TFilter filter, Action action)
        {
            actions[filter] = actions.TryGetValue(filter, out Action existsAction) ? existsAction - action : null;
            if (actions[filter] != null) return;

            actions.Remove(filter);
        }


        public void Register(TFilter filter)
        {
            NotifierEvents.Register(this);
            filters.Enqueue(filter);
        }
    }
}
