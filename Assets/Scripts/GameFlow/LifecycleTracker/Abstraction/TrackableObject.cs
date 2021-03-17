namespace Drawmasters
{
    public class TrackableObject : ITrackable
    {
        public ILifecycleTracker Tracker { get; }

        public TrackableObject()
        {
            Tracker = new LifecycleTracker(); 
        }

        ~TrackableObject()
        {
            Tracker.OnDestroy();
        }
    }
}