namespace Drawmasters
{
    public interface ITrackable
    {
        ILifecycleTracker Tracker { get; }
    }
}