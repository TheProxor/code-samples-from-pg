namespace Drawmasters.Levels
{
    // TODO rename
    public interface ILeftObjectHandler : IInitializable, IDeinitializable
    {
        void HandleLeftTarget(LevelTarget leftTarget);

        void HandleKilledTarget(LevelTarget killedTarget);
    }
}

