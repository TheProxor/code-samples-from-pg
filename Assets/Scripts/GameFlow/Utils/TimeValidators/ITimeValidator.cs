using System;


namespace Drawmasters.Utils
{
    public interface ITimeValidator : IInitializable, IDeinitializable
    {
        bool WasValidated { get; }

        DateTime ValidNow { get; }

        DateTime ValidUtcNow { get; }

        bool AllowTimeCheating { get; set; }

        DateTime CheatUtcNow { get; }

        DateTime LastBackgroundEnterRealUtcTime { get; }
    }
}
