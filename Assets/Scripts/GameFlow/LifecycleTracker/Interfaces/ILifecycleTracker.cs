namespace Drawmasters
{
    public interface ILifecycleTracker : IInitializable, IDeinitializable
    {
        void OnDestroy();
    }
}