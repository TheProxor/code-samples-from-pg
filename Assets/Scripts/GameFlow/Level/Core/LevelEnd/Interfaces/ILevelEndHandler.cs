using System;

namespace Drawmasters.Levels
{
    public interface ILevelEndHandler : IInitializable, IDeinitializable
    {
        event Action<LevelResult> OnEnded;
    }
}