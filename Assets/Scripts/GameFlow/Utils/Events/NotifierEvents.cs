using System.Collections.Generic;


namespace Core
{
    public interface IEvent
    {
        void Notify();
    }

    internal static class NotifierEvents
    {
        static NotifierEvents() => MonoBehaviourLifecycle.OnLateUpdate += Update;

        public static void Register(IEvent newEvent)
        {
            Events.Enqueue(newEvent);
        }

        private static void Update(float deltaTime)
        {
            while (Events.Count > 0)
            {
                Events.Dequeue().Notify();
            }
        }

        private static readonly Queue<IEvent> Events = new Queue<IEvent>();
    }
}
