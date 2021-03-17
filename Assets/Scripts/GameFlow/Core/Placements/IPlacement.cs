namespace Drawmasters.Advertising
{
    public interface IPlacement
    {
        bool CanShow { get; }

        string PlacementName { get; }
    }
}

