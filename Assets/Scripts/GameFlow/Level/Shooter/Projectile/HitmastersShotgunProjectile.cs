using System.Collections.Generic;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class HitmastersShotgunProjectile : Projectile
    {
        #region Fields

        [SerializeField] private CircleCollider2D triggerCollider = default;

        [SerializeField] private List<CollidableObjectType> typesToRebound = default;

        #endregion



        #region Propeties

        public override ProjectileType Type => ProjectileType.HitmastersShotgunBullet;


        protected override List<ProjectileComponent> CoreComponents
        {
            get
            {
                var result = base.CoreComponents;

                result.AddRange(new List<ProjectileComponent>
                {
                    new ShotgunTriggerImpuls(projectileCollisionNotifier),
                    new ProjectileConstShotComponent(),
                    new ProjectileRigidbodySettingsComponent(),
                    new ProjectileEnemyApplyRagdollComponent(projectileCollisionNotifier),
                    new ProjectileShooterDestroyOnCollisionComponent(typesThatDestroyProjectile,
                                                              physicalLevelObjectTypes,
                                                              projectileCollisionNotifier),
                    new ProjectileShooterTrail(),
                    new ProjectileSfxCollisionComponent(projectileCollisionNotifier, typesToRebound),
                    new ProjectileCircleColliderComponent(triggerCollider)
                });

                return result;
            }
        }

        #endregion
    }
}
