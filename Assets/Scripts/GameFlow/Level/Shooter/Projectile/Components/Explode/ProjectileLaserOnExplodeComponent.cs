using System;
using Modules.General;


namespace Drawmasters.Levels
{
    public class ProjectileLaserOnExplodeComponent : ProjectileComponent
    {
        #region Fields

        public static event Action<LevelTarget, float> OnShouldStartLaserDestroy;

        private readonly float laserDestroyDelay;

        #endregion



        #region Class lifecycle

        public ProjectileLaserOnExplodeComponent(float _laserDestroyDelay)
        {
            laserDestroyDelay = _laserDestroyDelay;
        }

        #endregion



        #region Methods

        public override void Initialize(Projectile mainProjectileValue, WeaponType type)
        {
            base.Initialize(mainProjectileValue, type);

            ProjectileExplodeApplyComponent.OnLevelTargetExploded += ProjectileExplodeApplyComponent_OnLevelTargetExploded;
        }


        public override void Deinitialize()
        {
            ProjectileExplodeApplyComponent.OnLevelTargetExploded -= ProjectileExplodeApplyComponent_OnLevelTargetExploded;

            Scheduler.Instance.UnscheduleAllMethodForTarget(this);

            base.Deinitialize();
        }

        #endregion



        #region Events handlers


        private void ProjectileExplodeApplyComponent_OnLevelTargetExploded(LevelTarget levelTarget)
        {
            ProjectileExplodeApplyComponent.OnLevelTargetExploded -= ProjectileExplodeApplyComponent_OnLevelTargetExploded;

            OnShouldStartLaserDestroy?.Invoke(levelTarget, laserDestroyDelay);
        }

        #endregion
    }
}
