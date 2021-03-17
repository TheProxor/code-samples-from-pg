using Drawmasters.Levels;
using Drawmasters.Levels.Data;
using Drawmasters.ServiceUtil.Interfaces;


namespace Drawmasters.ServiceUtil
{
    public class LevelEnvironment : ILevelEnvironment
    {
        #region ILevelEnvironment

        public LevelContext Context { get; private set; }

        public LevelProgress Progress { get; private set; }

        public void LoadEnvironment(GameMode mode,
                                    int levelIndex,
                                    bool isEditor,
                                    WeaponType loadedWeapon,
                                    GameMode sceneMode,
                                    LevelType levelType,
                                    Level.Data levelData,
                                    int projectilesCount,
                                    bool isPremiumProposalScene)
        {
            bool isHostage = levelData.levelObjectsData.Exists(e => Content.Management.FindLevelObject(e.index) is LevelHostage);

            Context = new LevelContext();
            Context.LoadContext(mode,
                                levelIndex,
                                isHostage,
                                isEditor,
                                loadedWeapon,
                                sceneMode,
                                levelType,
                                levelData.pathDistance,
                                projectilesCount,
                                isPremiumProposalScene);

            // TODO refactor
            bool needClearProgressData = Context.IsFirstSublevel;
            needClearProgressData &= (Context.SceneMode == GameMode.None) ||
                                     (Context.Mode.IsHitmastersLiveOps() && Context.SceneMode.IsProposalSceneMode());

            Progress = new LevelProgress();

            if (needClearProgressData)
            {
                Progress.StartNewProgress();
            }
            else
            {
                Progress.LoadProgress();
            }

            Progress.Subscribe();
        }


        public void Unload()
        {
            if (Context != null)
            {
                Context = null;
            }

            if (Progress != null)
            {
                Progress.Unsubscribe();
                if (Progress.LevelResultState != LevelResult.Reload)
                {
                    Progress.SaveData();    
                }
                Progress = null;
            }

        }

        #endregion
    }
}
