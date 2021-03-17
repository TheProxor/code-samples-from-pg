using System;
using System.Collections.Generic;
using Drawmasters.ServiceUtil;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class PetRocketProjectile : Projectile
    {
        #region Fields

        [Header("Physics notifier")]
        [SerializeField] private CollisionNotifier physicsCollisionNotifier = default;

        [Header("Projectile rebound")]
        [SerializeField] private List<CollidableObjectType> typesToRebound = default;
        [SerializeField] private List<CollidableObjectType> typesToExplodeOnCollision = default;

        #endregion



        #region Properties

        public override ProjectileType Type =>
            ProjectileType.PetRocket;


        protected override List<ProjectileComponent> CoreComponents
        {
            get
            {
                var result = base.CoreComponents;

                Func<string> vfxExplodeFunc = () =>
                {
                    PetSkinType petSkinType = GameServices.Instance.PlayerStatisticService.PlayerData.CurrentPetSkin;
                    PetWeaponSkinSettings petWeaponSkinSettings = IngameData.Settings.pets.weaponSkinSettings;
                    string projectileDestroyFxKey = petWeaponSkinSettings.FindProjectileDestroyFxKey(petSkinType);
                    return projectileDestroyFxKey;
                };

                Func<string> sfxExplodeFunc = () => string.Empty;
                float explodeDelay = IngameData.Settings.pets.levelSettings.projectileExplodeDelay;

                result.AddRange(new List<ProjectileComponent>()
                {
                    new ProjectilePetSkinComponent(mainRenderer),

                    new ProjectileExplodeApplyComponent(vfxExplodeFunc, sfxExplodeFunc),
                    new ProjectileExplodeOnCollisionComponent(typesToExplodeOnCollision, projectileCollisionNotifier, explodeDelay),

                    new SniperTriggerImpuls(projectileCollisionNotifier),
                    new ProjectileRigidbodySettingsComponent(),
                    new ProjectileEnemyApplyRagdollComponent(projectileCollisionNotifier),
                    new ProjectileConstShotComponent(),
                    new ProjectileColoredEnemyApplyRagdollComponent(projectileCollisionNotifier),
                    new ProjectilePetTrail(),
                    new ProjectileSfxCollisionComponent(physicsCollisionNotifier, typesToRebound),
                    new ProjectileSmashShakeComponent(),
                    new ProjectilePetDestroyOnCollisionComponent(typesThatDestroyProjectile,
                                                                 physicalLevelObjectTypes,
                                                                 projectileCollisionNotifier),
                });

                return result;
            }
        }

        #endregion
    }
}
