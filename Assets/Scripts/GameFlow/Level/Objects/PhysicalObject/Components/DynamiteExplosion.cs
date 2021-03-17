using UnityEngine;
using System;
using System.Collections.Generic;
using Modules.Sound;
using Drawmasters.Vibration;
using Drawmasters.Effects;
using Drawmasters.Statistics.Data;


namespace Drawmasters.Levels
{
    public class DynamiteExplosion : PhysicalLevelObjectComponent
    {
        #region Fields

        public static event Action<LevelTarget, LevelTargetLimbPart, DynamiteSettings.ExplosionData> OnLimbPartExploded;

        private List<CollidableObjectType> explosiveTypes;

        private DynamiteSettings.ExplosionData explosionData;
        private LayerMask explosionMask;

        private Collider2D[] castColliders;

        #endregion



        #region Methods

        public override void Initialize(CollisionNotifier notifier,
                                        Rigidbody2D rigidbody,
                                        PhysicalLevelObject _sourceObject)
        {
            base.Initialize(notifier, rigidbody, _sourceObject);

            if (IngameData.Settings.dynamiteSettings.TryFindExplosionData(sourceLevelObject.PhysicalData,
                                                                    out explosionData))
            {
                explosionMask = DynamiteSettings.ExplosionMask;
                explosiveTypes = new List<CollidableObjectType>(DynamiteSettings.ExplosibleTypes);

                castColliders = new Collider2D[128];
            }
        }


        public override void Enable()
        {
            sourceLevelObject.OnPreDestroy += SourceLevelObject_OnPreDestroy;
        }


        public override void Disable()
        {
            sourceLevelObject.OnPreDestroy -= SourceLevelObject_OnPreDestroy;
        }



        private void CheckExplosibleTargets(Collider2D[] colliders)
        {
            List<CollidableObject> explosible = new List<CollidableObject>();

            foreach (var c in colliders)
            {
                if (c == null ||
                    c == sourceLevelObject.MainCollider2D)
                {
                    continue;
                }

                CollidableObject collidable = c.GetComponent<CollidableObject>();

                if (collidable != null)
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
                if (enemy == null || (enemy.Type == LevelTargetType.Shooter && !PlayerData.IsUaKillingShootersEnabled))
                {
                    continue;
                }

                if (!enemy.Ragdoll2D.IsActive)
                {
                    enemy.ApplyRagdoll();

                    if (!enemy.Ragdoll2D.IsActive)
                    {
                        enemy.MarkHitted();
                    }
                }

                LevelTargetLimbPart part = c.GetComponent<LevelTargetLimbPart>();
                if (part == null)
                {
                    continue;
                }

                string boneName = part.BoneName;

                Rigidbody2D partRb = enemy.Ragdoll2D.GetRigidbody(boneName);

                if (partRb == null)
                {
                    continue;
                }

                OnLimbPartExploded?.Invoke(enemy, part, explosionData);

                affectedBodies.Add(partRb);

            }

            foreach (var enemyRigidbody in affectedBodies)
            {
                enemyRigidbody.velocity = Vector2.zero;
                enemyRigidbody.angularVelocity = 0f;

                ExplosionUtility.ApplyEnemyForce(enemyRigidbody,
                                                 sourceLevelObject.transform,
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
                                                sourceLevelObject.transform,
                                                explosionData);
                }
            }
        }


        private void Explode()
        {
            castColliders.Clear();

            Physics2D.OverlapCircleNonAlloc(sourceLevelObject.transform.position,
                                            explosionData.radius,
                                            castColliders,
                                            explosionMask);

            CheckExplosibleTargets(castColliders);

            EffectManager.Instance.PlaySystemOnce(EffectKeys.FxObjectBombExplosion,
                                                  sourceLevelObject.transform.position,
                                                  sourceLevelObject.transform.rotation);

            SoundManager.Instance.PlayOneShot(AudioKeys.Ingame.EXPLODE_1);

            VibrationManager.Play(IngameVibrationType.DynamiteExplosion);

            CameraShakeSettings.Shake shake = IngameData.Settings.cameraShakeSettings.dynamiteExplosion;

            IngameCamera.Instance.Shake(shake);
        }

        #endregion



        #region Events handlers

        void SourceLevelObject_OnPreDestroy(PhysicalLevelObject _levelObject)
        {
            if (sourceLevelObject != _levelObject ||
                sourceLevelObject.IsOutOfZone ||
                explosionData == null)
            {
                return;
            }

            Explode();
        }

        #endregion
    }
}
