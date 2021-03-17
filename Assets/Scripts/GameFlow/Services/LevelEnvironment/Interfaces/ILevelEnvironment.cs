using Drawmasters.Levels;
using Drawmasters.Levels.Data;


namespace Drawmasters.ServiceUtil.Interfaces
{
    public interface ILevelEnvironment
    {
        LevelContext Context { get; }

        LevelProgress Progress { get; }

        void LoadEnvironment(GameMode mode,
                             int levelIndex,
                             bool isEditor,
                             WeaponType loadedWeapon,
                             GameMode sceneMode,
                             LevelType levelType,
                             Level.Data levelData,
                             int projectilesCount,
                             bool isProposalPremiumScene);
        void Unload();
    }
}
