using Spine.Unity;


namespace Drawmasters.Levels
{
    public class HostageAnimation : EnemyAnimation
    {
        #region Properties

        protected override string CharacterLoseAnimation => animationNames.DefeatAnimation;

        #endregion



        #region Ctor

        public HostageAnimation(SkeletonAnimation _skeletonAnimation, EnemyAnimationNames _enemyAnimationNames) :
            base(_skeletonAnimation, _enemyAnimationNames)
        {
        }

        #endregion



        #region Events handlers

        protected override void Level_OnLevelStateChanged(LevelState levelState)
        {
            base.Level_OnLevelStateChanged(levelState);

            if (levelState == LevelState.AllTargetsHitted)
            {
                PlayAnimation(animationNames.RandomWinAnimation, true, MainIndex);
            }
        }

        #endregion
    }
}
