using Drawmasters.LevelsRepository;


namespace Drawmasters.Levels
{
    public interface IHeaderLoader
    {
        LevelHeader LoadedHeader { get; }

        LevelHeader LoadHeader(GameMode mode,
                               int index,
                               GameMode specialScene,
                               WeaponType weaponType);
    }
}

