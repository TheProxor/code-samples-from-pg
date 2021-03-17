using System.Collections.Generic;
using UnityEngine;
using Modules.Sound;
using Drawmasters.Effects;


namespace Drawmasters.Levels
{
    public abstract class ProjectileDestroyOnCollisionComponent : ProjectileComponent
    {
        #region Fields

        private readonly List<CollidableObjectType> typesThatDestroyProjectile;
        private readonly List<PhysicalLevelObjectType> physicalLevelObjectTypes;
        private readonly CollisionNotifier projectileCollisionNotifier;

        private string effectKeyOnDestroy;
        private string[] soundsEffectKeyOnDestroy;

        #endregion



        #region Properties

        protected abstract string EffectKeyOnDestroy { get; }

        protected abstract string[] SoundsEffectKeyOnDestroy { get; }

        #endregion



        #region Lifecycle

        public ProjectileDestroyOnCollisionComponent(List<CollidableObjectType> _typesThatDestroyProjectile,
                                                     List<PhysicalLevelObjectType> _physicalLevelObjectTypes,
                                                     CollisionNotifier _projectileCollisionNotifier)
        {
            typesThatDestroyProjectile = _typesThatDestroyProjectile;
            physicalLevelObjectTypes = _physicalLevelObjectTypes;
            projectileCollisionNotifier = _projectileCollisionNotifier;
        }

        #endregion



        #region Methods

        public override void Initialize(Projectile mainProjectileValue, WeaponType type)
        {
            base.Initialize(mainProjectileValue, type);

            projectileCollisionNotifier.OnCustomTriggerEnter2D += CollisionNotifier_OnCustomTriggerEnter2D;

            effectKeyOnDestroy = EffectKeyOnDestroy;
            soundsEffectKeyOnDestroy = SoundsEffectKeyOnDestroy;
        }


        public override void Deinitialize()
        {
            projectileCollisionNotifier.OnCustomTriggerEnter2D -= CollisionNotifier_OnCustomTriggerEnter2D;

            base.Deinitialize();
        }

        #endregion



        #region Events handlers

        private void CollisionNotifier_OnCustomTriggerEnter2D(GameObject reference, Collider2D collision)
        {
            CollidableObject collidableObject = collision.transform.GetComponent<CollidableObject>();

            if (collidableObject != null)
            {
                bool isDestroyType = typesThatDestroyProjectile.Contains(collidableObject.Type) ||
                                     ColorTypesSolutions.ShouldDestroyProjectileOnCollision(collidableObject.LevelTarget, mainProjectile.ColorType);

                if (isDestroyType)
                {
                    bool allowToDestroy = isDestroyType;

                    bool isPhysicalObject = (collidableObject.Type == CollidableObjectType.PhysicalObject);

                    if (isPhysicalObject)
                    {
                        PhysicalLevelObject physicalObject = collidableObject.PhysicalLevelObject;

                        allowToDestroy = (physicalObject != null) &&
                                         (physicalLevelObjectTypes.Contains(physicalObject.PhysicalData.type));
                    }

                    if (allowToDestroy)
                    {
                        EffectManager.Instance.PlaySystemOnce(effectKeyOnDestroy,
                                                              mainProjectile.transform.position,
                                                              mainProjectile.transform.rotation);

                        foreach (var soundKey in soundsEffectKeyOnDestroy)
                        {
                            SoundManager.Instance.PlaySound(soundKey);
                        }

                        mainProjectile.Destroy();
                    }
                }
            }
        }

        #endregion
    }
}
