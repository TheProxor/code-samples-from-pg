using Spine.Unity;

namespace Drawmasters.Levels
{
    public class BossAnimationBase : LevelTargetAnimation
    {
        #region Fields

        protected const int MoveAnimationIndex = 4;

        protected readonly BossEnemyAnimationNames bossAnimationNames;
        protected EnemyBossBase enemyBoss;


        #endregion



        #region Ctor

        public BossAnimationBase(SkeletonAnimation _skeletonAnimation, 
                                 BossEnemyAnimationNames _bossEnemyAnimationNames) : 
            base(_skeletonAnimation, _bossEnemyAnimationNames)
        {
            bossAnimationNames = _bossEnemyAnimationNames;
        }

        #endregion



        #region Overrided methods

        public override void Enable()
        {
            base.Enable();

            ClearAnimationState();

            skeletonAnimation.AnimationState.ClearTracks();
            skeletonAnimation.AnimationState.Update(default);
            skeletonAnimation.Skeleton.SetToSetupPose();

            levelTarget.OnHitted += LevelTarget_OnHitted;
            levelTarget.OnDefeated += LevelTarget_OnDefeated;
            levelTarget.OnShouldStartLaserDestroy += LevelTarget_OnShouldStartLaserDestroy;            

            enemyBoss = levelTarget as EnemyBossBase;
            if (enemyBoss == null)
            {
                CustomDebug.Log($" Wrong component. Supposed levelTarget as EnemyBoss");
                return;
            }

            enemyBoss.OnAppeared += EnemyBoss_OnAppeared;
        }


        public override void Disable()
        {
            levelTarget.OnHitted -= LevelTarget_OnHitted;
            levelTarget.OnDefeated -= LevelTarget_OnDefeated;
            levelTarget.OnShouldStartLaserDestroy -= LevelTarget_OnShouldStartLaserDestroy;

            if (enemyBoss != null)
            {
                enemyBoss.OnAppeared -= EnemyBoss_OnAppeared;
            }

            base.Disable();
        }

        #endregion



        #region Protected methods

        protected void PlayMockeryAnimation() =>
            PlayAnimation(bossAnimationNames.RandomMockeryAnimation, false, FaceIndex);

        #endregion



        #region Events handlers

        private void EnemyBoss_OnAppeared()
        {
            if (LevelStageController.CurrentStageIndex == 0)
            {
                PlayAnimation(bossAnimationNames.RandomGreetingMockeryAnimations, false, MoveAnimationIndex);
            }
            else
            {
                PlayMockeryAnimation();
            }

            AddAnimation(bossAnimationNames.IdleAnimationName, true, MoveAnimationIndex);
        }


        private void LevelTarget_OnHitted(LevelTarget anotherLevelTarget)
        {
            if (levelTarget == anotherLevelTarget)
            {
                skeletonAnimation.AnimationState.ClearTracks();

                PlayAnimation(bossAnimationNames.RandomAngryAnimation, false, MoveAnimationIndex);
                AddAnimation(bossAnimationNames.IdleAnimationName, true, MoveAnimationIndex);
            }
        }


        private void LevelTarget_OnDefeated(LevelTarget anotherLevelTarget)
        {
            skeletonAnimation.AnimationState.ClearTracks();

            PlayAnimation(bossAnimationNames.RandomFaceDeathAnimation, true, FaceIndex);
            PlayAnimation(bossAnimationNames.DefeatAnimation, false, MainIndex);

            skeletonAnimation.Update(0.0f);

            levelTarget.OnHitted -= LevelTarget_OnHitted;
            DisableOnAimAnimations();
        }


        private void LevelTarget_OnShouldStartLaserDestroy()
        {
            if (levelTarget.AllowVisualizeDamage)
            {
                levelTarget.OnShouldStartLaserDestroy -= LevelTarget_OnShouldStartLaserDestroy;

                PlayBurnAnimation();
            }
        }

        #endregion
    }
}
