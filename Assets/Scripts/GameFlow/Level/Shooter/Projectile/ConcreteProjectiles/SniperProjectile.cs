using System.Collections.Generic;
using Drawmasters.Statistics.Data;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class SniperProjectile : Projectile
    {
        #region Fields

        [Header("Physics notifier")]
        [SerializeField] private CollisionNotifier physicsCollisionNotifier = default;

        [Header("Projectile rebound")]
        [SerializeField] private List<CollidableObjectType> typesToRebound = default;

        #endregion



        #region Properties

        public override ProjectileType Type => ProjectileType.Arrow;


        protected override List<ProjectileComponent> CoreComponents
        {
            get
            {
                var result = base.CoreComponents;

                result.AddRange(new List<ProjectileComponent>()
                {
                    new ProjectileShooterSkinComponent(mainRenderer),
                    new SniperTriggerImpuls(projectileCollisionNotifier),
                    new ProjectileRigidbodySettingsComponent(),
                    new ProjectileTrajectoryConstShotComponent(),                    
                    new ProjectileColoredEnemyApplyRagdollComponent(projectileCollisionNotifier),
                    new ProjectileShooterTrail(),
                    new ProjectileSmashApplyComponent(),
                    new ProjectileSmashComponent(physicsCollisionNotifier),
                    new ProjectileEnemiesSmashComponent(projectileCollisionNotifier),
                    new ProjectileObjectsSmashComponent(projectileCollisionNotifier),
                    new ProjectileShooterDestroyOnCollisionComponent(typesThatDestroyProjectile,
                                                                     physicalLevelObjectTypes,
                                                                     projectileCollisionNotifier),
                    new ProjectileSfxCollisionComponent(physicsCollisionNotifier, typesToRebound),

                    new ProjectileStayApplyComponent(),
                    new ProjectileMonolithStayComponent(projectileCollisionNotifier),
                    new ProjectileLevelTargetStuckComponent(projectileCollisionNotifier),
                    new ProjectileSmashShakeComponent(),
                    new ProjectileLaserHitComponent(mainRenderer),
                    new ProjectileDestroyBossLevelComponent(),
                    new ProjectileDestroyBonusLevelComponent()
                });

                if (PlayerData.IsUaKillingShootersEnabled)
                {
                    result.Add(new ProjectileShooterApplyRagdollComponent(projectileCollisionNotifier));
                }

                return result;
            }
        }

        #endregion
    }
}
