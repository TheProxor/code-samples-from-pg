using System.Collections.Generic;
using UnityEngine;

namespace Drawmasters.Levels
{
    public class HitmastersSniperBulletProjectile : Projectile
    {
        #region Fields

        [Header("Physics notifier")]
        [SerializeField] private CollisionNotifier physicsCollisionNotifier = default;

        [Header("Projectile rebound")]
        [SerializeField] private List<CollidableObjectType> typesToRebound = default;

        [Header("Colliders")]
        [SerializeField] private CircleCollider2D triggerCollider = default;
        [SerializeField] private CircleCollider2D physicsCollider = default;

        #endregion



        #region Abstract implementation

        public override ProjectileType Type => ProjectileType.HitmastersSniperBullet;

        #endregion



        #region Overrided methods

        protected override List<ProjectileComponent> CoreComponents
        {
            get
            {
                List<ProjectileComponent> components = base.CoreComponents;

                components.AddRange(new List<ProjectileComponent>
                {
                    new SniperTriggerImpuls(physicsCollisionNotifier),
                    new ProjectileRigidbodySettingsComponent(),
                    new ProjectileEnemyApplyRagdollComponent(projectileCollisionNotifier),
                    new ProjectileShooterTrail(),
                    new ProjectileShooterDestroyOnCollisionComponent(typesThatDestroyProjectile,
                                                              physicalLevelObjectTypes,
                                                              projectileCollisionNotifier),
                    new ProjectileSfxCollisionComponent(physicsCollisionNotifier, typesToRebound),
                    new ProjectileLaserHitComponent(mainRenderer),
                    new ProjectileDestroyBossLevelComponent(),
                    new ProjectileConstShotComponent(),
                    new ProjectileReboundComponent(physicsCollisionNotifier, typesToRebound),
                    new ProjectileCollisionVelocitySaveComponent(physicsCollisionNotifier, mainRigidbody2D),
                    new ProjectilePhysicsMaterialComponent(),
                    new ProjectileRotation(),
                    new ProjectileCircleColliderComponent(triggerCollider),
                    new ProjectileCircleColliderComponent(physicsCollider)
                });

                return components;
            }
        }

        #endregion
    }
}
