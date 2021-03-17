using System;

namespace Drawmasters.Levels
{
    public interface ILevelStateChangeReporter
    {
        event Action<LevelState> OnLevelStateChanged;
    }
}

