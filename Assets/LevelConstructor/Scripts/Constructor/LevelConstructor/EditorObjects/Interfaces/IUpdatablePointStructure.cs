using System;


namespace Drawmasters.LevelConstructor
{
    public interface IUpdatablePointStructure : IPointStructure
    {
        event Action<IUpdatablePointStructure> OnPointsUpdate;
    }
}