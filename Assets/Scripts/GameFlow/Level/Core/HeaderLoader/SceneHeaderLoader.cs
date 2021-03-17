using Drawmasters.LevelsRepository;


namespace Drawmasters.Levels
{
    public class SceneHeaderLoader : IHeaderLoader
    {
        #region IHeaderLoader

        public LevelHeader LoadedHeader { get; private set; }


        public LevelHeader LoadHeader(GameMode mode,
                                      int index,
                                      GameMode specialScene,
                                      WeaponType weaponType)
        {
            LoadedHeader = LevelsContainer.GetSceneHeader(specialScene);

            return LoadedHeader;
        }

        #endregion
    }
}