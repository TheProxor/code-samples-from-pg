using UnityEngine;
using System;
using System.Collections.Generic;
using Modules.Sound;
using Drawmasters.Vibration;
using Drawmasters.Effects;


namespace Drawmasters.Levels
{
    // hack copypaste from physical level object component
    public class ProjectileExplodeApplyComponent : ProjectileComponent
    {
        #region Fields

        public static event Action<LevelTarget> OnLevelTargetExploded;
        public static event Action<LevelTarget, float, string> OnShouldChopOffLimb;
        
        private readonly Func<string> vfxKeyOnExplodeFunc;
        private readonly Func<string> sfxKeyOnExplodeFunc;

        private List<CollidableObjectType> explosiveTypes;

        private DynamiteSettings.ExplosionData explosionData;
        private LayerMask explosionMask;

        private Collider2D[] castColliders;



        #endregion



        #region Class lifecycle

        public ProjectileExplodeApplyComponent(Func<string> _vfxKeyOnExplode,
                                               Func<string> _sfxKeyOnExplode)
        {
            vfxKeyOnExplodeFunc = _vfxKeyOnExplode;
            sfxKeyOnExplodeFunc = _sfxKeyOnExplode;
        }

        #endregion



        #region Methods

        public override void Initialize(Projectile mainProjectileValue, WeaponType type)
        {
            base.Initialize(mainProjectileValue, type);

            if (IngameData.Settings.dynamiteSettings.TryFindExplosionData(mainProjectile.Type,
                                                                    out explosionData))
            {
                explosionMask = DynamiteSettings.ExplosionMask;
                explosiveTypes = new List<CollidableObjectType>(DynamiteSettings.ExplosibleTypes);

                castColliders = new Collider2D[128];

            }

            ProjectileExplodeOnCollisionComponent.OnShouldExplode += ProjectileExplodeOnCollisionComponent_OnShouldExplode;
        }


        public override void Deinitialize()
        {
            ProjectileExplodeOnCollisionComponent.OnShouldExplode -= ProjectileExplodeOnCollisionComponent_OnShouldExplode;

            base.Deinitialize();
        }


        private void CheckExplosibleTargets(Collider2D[] colliders)
        {
            List<CollidableObject> explosible = new List<CollidableObject>();

            foreach (var c in colliders)
            {
                if (c != null && c.TryGetComponent(out CollidableObject collidable))
                {
                    bool isExplosibleObject = explosiveTypes.Contains(collidable.Type);

                    if (isExplosibleObject)
                    {
                        explosible.Add(collidable);
                    }
                }
            }

            ApplyExplodeAtEnemies(explosible);
            ApplyExplodeAtPhysicsObjects(explosible);
        }



        private void ApplyExplodeAtEnemies(List<CollidableObject> collidable)
        {
            List<Rigidbody2D> affectedBodies = new List<Rigidbody2D>();

            foreach (var c in collidable)
            {
                if (c.Type != CollidableObjectType.EnemyStand &&
                    c.Type != CollidableObjectType.EnemyTrigger)
                {
                    continue;
                }

                LevelTarget enemy = c.LevelTarget;
                if (enemy == null)
                {
                    continue;
                }

                if (!enemy.Ragdoll2D.IsActive)
                {
                    enemy.ApplyRagdoll();

                    bool isBossEnemy = enemy is EnemyBoss;
                    
                    if (!enemy.Ragdoll2D.IsActive && 
                        !isBossEnemy)
                    {
                        enemy.MarkHitted();
                    }

                    OnLevelTargetExploded?.Invoke(enemy);
                }

                LevelTargetLimbPart part = c.GetComponent<LevelTargetLimbPart>();
                if (part == null)
                {
                    continue;
                }

                string boneName = part.BoneName;
                float damage = float.MaxValue;

                Rigidbody2D partRb = enemy.Ragdoll2D.GetRigidbody(boneName);

                if (partRb == null)
                {
                    continue;
                }

                bool isLimbDataExists = enemy.Settings.IsLimbDataExists(boneName);
                if (isLimbDataExists)
                {
                    OnShouldChopOffLimb?.Invoke(enemy, damage, boneName);
                }

                affectedBodies.Add(partRb);

            }

            foreach (var enemyRigidbody in affectedBodies)
            {
                enemyRigidbody.velocity = Vector2.zero;
                enemyRigidbody.angularVelocity = 0f;

                ExplosionUtility.ApplyEnemyForce(enemyRigidbody,
                                                 mainProjectile.transform,
                                                 explosionData);
            }
        }


        private void ApplyExplodeAtPhysicsObjects(List<CollidableObject> collidable)
        {
            foreach (var c in collidable)
            {
                if (c.Type != CollidableObjectType.PhysicalObject)
                {
                    continue;
                }

                PhysicalLevelObject physical = c.PhysicalLevelObject;

                if (physical == null)
                {
                    continue;
                }

                bool isDestroyed = default;

                isDestroyed |= physical.Strength <= explosionData.damage;
                isDestroyed |= physical.PhysicalData.type == PhysicalLevelObjectType.Dynamite;

                if (isDestroyed)
                {
                    physical.DestroyObject();
                }
                else
                {
                    ExplosionUtility.ApplyForce(physical.Rigidbody2D,
                                                mainProjectile.transform,
                                                explosionData);
                }
            }
        }


        private void Explode()
        {
            castColliders.Clear();

            Physics2D.OverlapCircleNonAlloc(mainProjectile.transform.position,
                                            explosionData.radius,
                                            castColliders,
                                            explosionMask);

            CheckExplosibleTargets(castColliders);

            string vfxKeyOnExplode = vfxKeyOnExplodeFunc();
            EffectManager.Instance.PlaySystemOnce(vfxKeyOnExplode,
                                                  mainProjectile.transform.position,
                                                  mainProjectile.transform.rotation);

            string sfxKeyOnExplode = sfxKeyOnExplodeFunc();
            SoundManager.Instance.PlayOneShot(sfxKeyOnExplode);

            VibrationManager.Play(IngameVibrationType.DynamiteExplosion);

            CameraShakeSettings.Shake shake = IngameData.Settings.cameraShakeSettings.dynamiteExplosion;

            IngameCamera.Instance.Shake(shake);
        }

        #endregion



        #region Events handlers


        private void ProjectileExplodeOnCollisionComponent_OnShouldExplode(Projectile projectile, CollidableObject collidableObject)
        {
            if (mainProjectile != projectile ||
                explosionData == null)
            {
                return;
            }

            Explode();
        }

        #endregion
    }
}
