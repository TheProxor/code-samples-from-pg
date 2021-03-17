using Modules.General;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class ProjectileShooterApplyRagdollComponent : ProjectileApplyRagdollComponent
    {
        private bool isMinTimeForSelfCollisionPassed;
        private bool isProjectileExitFromShooter;


        private bool AllowSelfColorCollision => isMinTimeForSelfCollisionPassed &&
                                                isProjectileExitFromShooter;


        public ProjectileShooterApplyRagdollComponent(CollisionNotifier _collisionNotifier) : base(_collisionNotifier)
        {
        }



        public override void Initialize(Projectile mainProjectileValue, WeaponType mode)
        {
            base.Initialize(mainProjectileValue, mode);

            isMinTimeForSelfCollisionPassed = false;
            isProjectileExitFromShooter = false;

            mainProjectile.ProjectileCollisionNotifier.OnCustomTriggerExit2D += ProjectileCollisionNotifier_OnCustomTriggerExit2D;

            float collisionAllowDelay = IngameData.Settings.shooter.collision.delayToSelfKillAllow;
            Scheduler.Instance.CallMethodWithDelay(this, () => isMinTimeForSelfCollisionPassed = true, collisionAllowDelay);
        }


        private void ProjectileCollisionNotifier_OnCustomTriggerExit2D(GameObject go, Collider2D collider2D)
        {
            CollidableObject collidableObject = collider2D.GetComponent<CollidableObject>();

            if (collidableObject == null)
            {
                return;
            }

            Shooter shooter = collidableObject.LevelTarget as Shooter;

            if (shooter != null && shooter.ColorType == mainProjectile.ColorType)
            {
                isProjectileExitFromShooter = true;
                StopCheckShootersLeft();
            }
        }


        public override void Deinitialize()
        {
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
            StopCheckShootersLeft();

            base.Deinitialize();
        }


        protected override bool CanHitTarget(LevelTarget levelTarget, CollidableObjectType collidableObjectType)
        {
            bool result = default;

            if (collidableObjectType == CollidableObjectType.EnemyStand &&
                levelTarget.Type == LevelTargetType.Shooter)
            {
                result = (levelTarget.ColorType == mainProjectile.ColorType) ?
                    AllowSelfColorCollision : ColorTypesSolutions.CanHitShooter();
            }

            return result;
        }


        private void StopCheckShootersLeft() =>
            mainProjectile.ProjectileCollisionNotifier.OnCustomTriggerExit2D -= ProjectileCollisionNotifier_OnCustomTriggerExit2D;
    }
}
