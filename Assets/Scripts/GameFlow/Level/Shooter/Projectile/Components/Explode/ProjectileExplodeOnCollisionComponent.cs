using System;
using System.Collections.Generic;
using UnityEngine;
using Drawmasters.ServiceUtil;
using Modules.General;


namespace Drawmasters.Levels
{
    public class ProjectileExplodeOnCollisionComponent : ProjectileComponent
    {
        #region Fields

        public static event Action<Projectile, CollidableObject> OnShouldExplode;

        private readonly List<CollidableObjectType> collisionTypes;
        private readonly CollisionNotifier projectileCollisionNotifier;
        private readonly float explodeDelay;

        private LevelProjectileController projectileController;

        #endregion



        #region Lifecycle

        public ProjectileExplodeOnCollisionComponent(List<CollidableObjectType> _typesThatDestroyProjectile,
                                                     CollisionNotifier _projectileCollisionNotifier,
                                                     float _explodeDelay = 0.0f)
        {
            collisionTypes = _typesThatDestroyProjectile;
            projectileCollisionNotifier = _projectileCollisionNotifier;
            explodeDelay = _explodeDelay;

            projectileController = GameServices.Instance.LevelControllerService.Projectile;
        }

        #endregion



        #region Methods

        public override void Initialize(Projectile mainProjectileValue, WeaponType type)
        {
            base.Initialize(mainProjectileValue, type);

            projectileCollisionNotifier.OnCustomTriggerEnter2D += CollisionNotifier_OnCustomTriggerEnter2D;
        }


        public override void Deinitialize()
        {
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);

            projectileCollisionNotifier.OnCustomTriggerEnter2D -= CollisionNotifier_OnCustomTriggerEnter2D;

            base.Deinitialize();
        }

        #endregion



        #region Events handlers

        private void CollisionNotifier_OnCustomTriggerEnter2D(GameObject reference, Collider2D collision)
        {
            if (collision.transform.TryGetComponent(out CollidableObject collidableObject))
            {
                bool isDestroyType = collisionTypes.Contains(collidableObject.Type);

                if (collidableObject.Projectile != null)
                {
                    isDestroyType = ColorTypesSolutions.ShouldExplodeProjectiles(mainProjectile, collidableObject.Projectile);
                }
                else if (collidableObject.LevelTarget != null)
                {
                    isDestroyType = collidableObject.LevelTarget.Type != LevelTargetType.Boss &&
                                    !collidableObject.LevelTarget.IsHitted;

                    if (isDestroyType)
                    {
                        const string targetRootBoneName = "body_char";
                        LevelTargetLimb levelTargetLimb = collidableObject.LevelTarget.Limbs.Find(e => targetRootBoneName.Equals(e.RootBoneName, StringComparison.Ordinal));
                        mainProjectile.transform.position = levelTargetLimb.transform.position;
                    }
                }

                if (isDestroyType)
                {
                    projectileCollisionNotifier.OnCustomTriggerEnter2D -= CollisionNotifier_OnCustomTriggerEnter2D;

                    // TODO: add delay logic
                    //Scheduler.Instance.CallMethodWithDelay(this, () =>
                    //{
                        OnShouldExplode?.Invoke(mainProjectile, collidableObject);
                        mainProjectile.Destroy();

                        if (collidableObject.Projectile != null)
                        {
                            bool isAnySameColorRocketsOnLevel = projectileController.IsAnyActiveProjectileExists(mainProjectile.Type, mainProjectile.ColorType);
                            if (!isAnySameColorRocketsOnLevel)
                            {
                                collidableObject.Projectile.Destroy();
                            }
                        }
                    //}, explodeDelay);
                }
            }
        }

        #endregion
    }
}

