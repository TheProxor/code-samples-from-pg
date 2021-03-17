using Spine.Unity;

namespace Drawmasters.Levels
{
    public class BossAnimation : BossAnimationBase
    {
        #region Fields        

        protected EnemyBoss attackedBoss;

        #endregion



        #region Ctor

        public BossAnimation(SkeletonAnimation _skeletonAnimation, 
            BossEnemyAnimationNames _bossEnemyAnimationNames) 
            : base(_skeletonAnimation, _bossEnemyAnimationNames) { }

        #endregion



        #region Methods        


        public override void Enable()
        {
            base.Enable();

            LevelTargetRocketLaunchComponent.OnRocketsLaunched += LevelTargetRocketLaunchComponent_OnRocketsLaunched;
            ProjectileExplodeOnCollisionComponent.OnShouldExplode += ProjectileExplodeOnCollisionComponent_OnShouldExplode;
            ProjectileExplodeApplyComponent.OnLevelTargetExploded += ProjectileExplodeApplyComponent_OnLevelTargetExploded;

            attackedBoss = levelTarget as EnemyBoss;
            if (attackedBoss == null)
            {
                CustomDebug.Log($" Wrong component. Supposed levelTarget as EnemyBoss");
                return;
            }

            attackedBoss.OnStartCome += PlayComeAnimation;
            attackedBoss.OnStartLeave += EnemyBoss_OnStartLeave;
        }


        public override void Disable()
        {
            LevelTargetRocketLaunchComponent.OnRocketsLaunched -= LevelTargetRocketLaunchComponent_OnRocketsLaunched;
            ProjectileExplodeOnCollisionComponent.OnShouldExplode -= ProjectileExplodeOnCollisionComponent_OnShouldExplode;
            ProjectileExplodeApplyComponent.OnLevelTargetExploded -= ProjectileExplodeApplyComponent_OnLevelTargetExploded;

            if (attackedBoss != null)
            {
                attackedBoss.OnStartCome -= PlayComeAnimation;
                attackedBoss.OnStartLeave -= EnemyBoss_OnStartLeave;
            }

            base.Disable();
        }       


        private void PlayComeAnimation(float duration)
        {
            PlayAnimation(bossAnimationNames.commingAnimation, true, MoveAnimationIndex);

            float comeDuration = duration - skeletonAnimation.skeleton.Data.FindAnimation(bossAnimationNames.endCommingAnimation).Duration;
            AddAnimation(bossAnimationNames.endCommingAnimation, false, MoveAnimationIndex, comeDuration);
        }

        #endregion



        #region Events handlers

        protected override void Level_OnLevelStateChanged(LevelState levelState)
        {
            base.Level_OnLevelStateChanged(levelState);

            if (levelState == LevelState.ReturnToInitial)
            {
                AddAnimation(bossAnimationNames.IdleAnimationName, true, MoveAnimationIndex);
            }
        }                


        private void LevelTargetRocketLaunchComponent_OnRocketsLaunched(RocketLaunchData.Data[] data)
        {
            PlayAnimation(bossAnimationNames.RandomShotAnimation, false, FaceIndex);
        }


        private void ProjectileExplodeOnCollisionComponent_OnShouldExplode(Projectile projectile, CollidableObject collidableObject)
        {
            if (projectile.Type == ProjectileType.BossRocket && collidableObject.Type == CollidableObjectType.Projectile)
            {
                PlayAnimation(bossAnimationNames.RandomProjectileDestroyAnimation, false, FaceIndex);
            }
        }


        private void ProjectileExplodeApplyComponent_OnLevelTargetExploded(LevelTarget levelTarget)
        {
            if (levelTarget.Type.IsFriendly())
            {
                PlayMockeryAnimation();
            }
        }        


        private void EnemyBoss_OnStartLeave()
        {
            if (string.IsNullOrEmpty(bossAnimationNames.beginLeavingAnimation) ||
                string.IsNullOrEmpty(bossAnimationNames.leavingAnimation))
            {
                CustomDebug.Log($"No leaving animations found");
                return;
            }
            PlayAnimation(bossAnimationNames.beginLeavingAnimation, false, MoveAnimationIndex);

            float leave = skeletonAnimation.skeleton.Data.FindAnimation(bossAnimationNames.beginLeavingAnimation).Duration;
            AddAnimation(bossAnimationNames.leavingAnimation, true, MoveAnimationIndex, leave);
        }

        #endregion
    }
}
