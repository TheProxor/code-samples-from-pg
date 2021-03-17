using System;

namespace Drawmasters.Levels
{
    public interface IShotModule : IInitializable, IDeinitializable
    {
        event Action OnShotReady;
    }
}
