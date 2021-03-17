using Drawmasters.LevelsRepository;


namespace Drawmasters.Levels
{
    public class EditorHeaderLoader : IHeaderLoader
    {
        public LevelHeader LoadedHeader { get; private set; }


        public LevelHeader LoadHeader(GameMode mode,
                                      int index,
                                      GameMode specialScene,
                                      WeaponType weaponType)
        {
            LoadedHeader = LevelsContainer.GetHeader(mode, index);

            return LoadedHeader;
        }
    }
}