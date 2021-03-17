namespace Drawmasters.Levels
{
    public interface IFieldCheckRule : IInitializable, IDeinitializable
    {
        bool IsMatching(LevelTarget checkingObject);
    }
}

