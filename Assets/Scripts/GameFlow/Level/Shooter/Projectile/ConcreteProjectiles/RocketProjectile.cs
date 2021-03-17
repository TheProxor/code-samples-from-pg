using System;
using System.Collections.Generic;
using Drawmasters.Statistics.Data;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class RocketProjectile : Projectile
    {
        #region Fields

        [Header("Projectile rebound")]
        [SerializeField] private List<CollidableObjectType> typesToRebound = default;
        [Header("Projectile rebound")]
        [SerializeField] private List<CollidableObjectType> typesToExplodeOnCollision = default;

        #endregion



        #region Properties

        public override ProjectileType Type => ProjectileType.BossRocket;


        protected override List<ProjectileComponent> CoreComponents
        {
            get
            {
                var result = base.CoreComponents;

                Func<string> vfxExplodeFunc = () => EffectKeys.FxObjectBombExplosion;
                Func<string> sfxExplodeFunc = () => AudioKeys.Ingame.ROCKETS_EXPLOSIONS;

                result.AddRange(new List<ProjectileComponent>()
                {
                    new ProjectileBossSkinComponent(mainRenderer),
                    new SniperTriggerImpuls(projectileCollisionNotifier),
                    new ProjectileRigidbodySettingsComponent(),
                    new ProjectileTrajectoryConstShotComponent(),
                    new ProjectileRocketTrail(),
                    new ProjectileSfxCollisionComponent(projectileCollisionNotifier, typesToRebound),
                    new ProjectileSmashShakeComponent(),
                    new ProjectileExplodeApplyComponent(vfxExplodeFunc, sfxExplodeFunc),
                    new ProjectileExplodeOnCollisionComponent(typesToExplodeOnCollision, projectileCollisionNotifier),
                    new ProjectileReturnToInitialFxRocketComponent()
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
