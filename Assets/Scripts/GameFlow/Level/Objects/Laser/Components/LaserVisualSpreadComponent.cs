using System;
using System.Collections;
using System.Collections.Generic;
using Drawmasters.Effects;
using Drawmasters.Utils;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class LaserVisualSpreadComponent : LaserComponent
    {
        #region Fields

        public static event Action<PortalObject, Vector2, Vector3, LaserLevelObject> OnShouldTeleportLaser; // entered , dir, pos, laser

        private static readonly int raycastMask = ~(LayerMask.GetMask(PhysicsLayers.Shooter) &
                                                    LayerMask.GetMask(PhysicsLayers.Acid));


        private static readonly List<CollidableObjectType> AllowDestroyTypes = new List<CollidableObjectType> { CollidableObjectType.EnemyStand,
                                                                                                               CollidableObjectType.EnemyTrigger,
                                                                                                               CollidableObjectType.PhysicalObject,
                                                                                                               CollidableObjectType.Spikes,
                                                                                                               CollidableObjectType.Projectile,
                                                                                                               CollidableObjectType.Portal
                                                                                                               };

        private static readonly List<CollidableObjectType> StopRayTypes = new List<CollidableObjectType> { CollidableObjectType.Monolith
                                                                                                               };

        private static readonly List<PhysicalLevelObjectType> ExceptedPhysicalLevelObjectTypes = new List<PhysicalLevelObjectType> { PhysicalLevelObjectType.Monolit };

        private LaserSettings laserSettings;

        private LineFx lineFx;
        private LineFx startLineFx;
        private EffectHandler lineFxCollision;


        private Coroutine teleportSpreadRoutine;
        private LineFx teleportSpreadLineFx;
        private EffectHandler teleportStartLineFxCollision;
        private EffectHandler teleportLineFxCollision;
        private Vector3 teleportPosition;
        private Vector3 teleportDirection;

        #endregion



        #region Methods

        public override void Enable()
        {
            lineFx = new LineFx();
            startLineFx = new LineFx();

            teleportSpreadLineFx = new LineFx();

            laserSettings = IngameData.Settings.laserSettings;

            startLineFx.Initialize(laserSettings.startLineFxSettings, laser.FxRoot);
            startLineFx.Play();

            PlayStartRayFx(lineFx, laser.transform);
            PlayCollisionFx(ref lineFxCollision, laser.transform);

            lineFx.EffectHandlerTransform.localPosition = Vector3.zero;
            lineFx.EffectHandlerTransform.localEulerAngles = Vector3.zero;

            startLineFx.EffectHandlerTransform.localPosition = Vector3.zero;
            startLineFx.EffectHandlerTransform.localEulerAngles = Vector3.zero;

            MonoBehaviourLifecycle.OnUpdate += MonoBehaviourLifecycle_OnUpdate;

            PortalController.OnAllowTeleportRay += PortalController_OnAllowTeleportRay;
            laser.OnShouldDisableLaser += DisableLaser;
        }


        public override void Disable()
        {
            laser.OnShouldDisableLaser -= DisableLaser;

            DisableLaser();
        }


        private void DisableLaser()
        {
            MonoBehaviourLifecycle.OnUpdate -= MonoBehaviourLifecycle_OnUpdate;

            PortalController.OnAllowTeleportRay -= PortalController_OnAllowTeleportRay;

            MonoBehaviourLifecycle.StopPlayingCorotine(teleportSpreadRoutine);
            teleportSpreadRoutine = null;

            StopFx(teleportStartLineFxCollision);
            StopFx(teleportLineFxCollision);
            teleportSpreadLineFx.Deinitialize();

            StopFx(lineFxCollision);
            lineFx.Deinitialize();
            startLineFx.Deinitialize();
        }


        private float HitFirstObject(Vector3 position, Vector2 directionVector, bool isTeleported = false)
        {
            RaycastHit2D[] hits = Physics2D.BoxCastAll(position, Vector2.one.SetX(laserSettings.shotRendererWidth), default, directionVector, float.MaxValue, raycastMask);
            ILaserDestroyable hittedObject = default;

            float rayDistance = laserSettings.defaultRayDistance;
            bool wasPortalFound = false;

            foreach (var hit in hits)
            {
                if (hit.collider != null)
                {
                    CollidableObject collidableObject = hit.collider.GetComponent<CollidableObject>();

                    if (collidableObject != null)
                    {
                        if (collidableObject.Type == CollidableObjectType.Pet)
                        {
                            continue;
                        }

                        if (!isTeleported)
                        {
                            if (collidableObject.PortalObject != null)
                            {
                                OnShouldTeleportLaser?.Invoke(collidableObject.PortalObject, directionVector, hit.point, laser);
                                rayDistance = hit.distance;
                                wasPortalFound = true;
                                break;
                            }
                        }

                        bool isStopObject = StopRayTypes.Contains(collidableObject.Type);
                        bool allowDestroy = AllowDestroyTypes.Contains(collidableObject.Type);

                        if (isStopObject)
                        {
                            rayDistance = hit.distance;
                        }


                        if (isStopObject || !allowDestroy)
                        {
                            break;
                        }

                        if (collidableObject.Projectile != null)
                        {
                            hittedObject = collidableObject.Projectile;
                        }

                        if (collidableObject.LevelTarget != null)
                        {
                            LevelTarget pulledLevelTarget = collidableObject.LevelTarget;

                            pulledLevelTarget.ApplyRagdoll();

                            LevelTargetLimbPart collidedLimbPart = collidableObject.GetComponent<LevelTargetLimbPart>();
                            LevelTargetLimb collidedLimb = pulledLevelTarget.Limbs.Find(e => e.LimbParts.Contains(collidedLimbPart));

                            bool isChoppedOffLimb = collidedLimb != null && pulledLevelTarget.IsChoppedOffLimb(collidedLimb.RootBoneName);
                            hittedObject = isChoppedOffLimb ? collidedLimb : (ILaserDestroyable)collidableObject.LevelTarget;
                        }
                        else if (collidableObject.AnyLevelObject != null)
                        {
                            hittedObject = collidableObject.AnyLevelObject as ILaserDestroyable;

                            if (collidableObject.PhysicalLevelObject != null &&
                                ExceptedPhysicalLevelObjectTypes.Contains(collidableObject.PhysicalLevelObject.PhysicalData.type))
                            {
                                rayDistance = hit.distance;
                                break;
                            }
                        }

                        if (!hittedObject.IsNull())
                        {
                            laser.StartDestroyObject(hittedObject);

                            if (!(hittedObject is Projectile))
                            {
                                rayDistance = hit.distance;
                                break;
                            }
                        }
                    }
                }
            }

            if (!isTeleported && !wasPortalFound && teleportSpreadRoutine != null)
            {
                StopFx(teleportLineFxCollision);
                StopFx(teleportStartLineFxCollision);

                MonoBehaviourLifecycle.StopPlayingCorotine(teleportSpreadRoutine);
                teleportSpreadLineFx.Deinitialize();
                teleportSpreadRoutine = null;
            }

            return rayDistance;
        }


        private IEnumerator TeleportSpreadRoutine()
        {
            teleportSpreadLineFx.Deinitialize();
            PlayStartRayFx(teleportSpreadLineFx, null);
            PlayCollisionFx(ref teleportLineFxCollision, null);
            PlayCollisionFx(ref teleportStartLineFxCollision, null);

            while (true)
            {
                teleportSpreadLineFx.EffectHandlerTransform.position = teleportPosition;

                float angle = Mathf.Atan2(-teleportDirection.x, teleportDirection.y) * Mathf.Rad2Deg + 90f;
                teleportSpreadLineFx.EffectHandlerTransform.localEulerAngles = Vector3.forward * angle;

                float rayDistance = HitFirstObject(teleportPosition, teleportDirection, true);
                teleportSpreadLineFx.RecalculateDistance(rayDistance);

                SetCollisionFxPosition(teleportLineFxCollision, teleportSpreadLineFx, rayDistance);
                SetCollisionFxPosition(teleportStartLineFxCollision, teleportSpreadLineFx, default);

                yield return null;
            }
        }


        private void DrawShotDirection(float distance)
        {
            if (lineFx != null)
            {
                lineFx.RecalculateDistance(distance);
                startLineFx.RecalculateDistance(distance);

                SetCollisionFxPosition(lineFxCollision, lineFx, distance);
                SetCollisionFxPosition(lineFxCollision, lineFx, distance);
            }
        }


        private void SetCollisionFxPosition(EffectHandler collisionHandler, LineFx rayFx, float distance)
        {
            if (collisionHandler != null &&
                rayFx != null &&
                rayFx.EffectHandlerTransform != null &&
                laserSettings != null)
            {
                Vector3 endFxCollisionPosition = rayFx.EffectHandlerTransform.position + rayFx.EffectHandlerTransform.right * (distance + laserSettings.collisionFxOffset);

                collisionHandler.transform.position = endFxCollisionPosition;
            }
        }


        private void PlayStartRayFx(LineFx rayFx, Transform parent) =>
            rayFx.Initialize(laserSettings.rayLineFxSettings, parent);


        private void PlayCollisionFx(ref EffectHandler collisionHandler, Transform parent)
        {
            StopFx(collisionHandler);
            collisionHandler = EffectManager.Instance.CreateSystem(EffectKeys.FxObjectLaserWallCollision,
                                                                   true,
                                                                   Vector3.zero,
                                                                   parent: parent,
                                                                   shouldOverrideLoops: false);
        }


        private void StopFx(EffectHandler handler)
        {
            if (handler != null && !handler.InPool)
            {
                EffectManager.Instance.PoolHelper.PushObject(handler);
            }
        }

        #endregion



        #region Events handlers

        private void MonoBehaviourLifecycle_OnUpdate(float deltaTime)
        {
            float rayDistance = HitFirstObject(laser.FxRoot.position, lineFx.EffectHandlerTransform.right);

            DrawShotDirection(rayDistance);
        }


        private void PortalController_OnAllowTeleportRay(LaserLevelObject anotherLaser, Vector3 position, Vector2 direction)
        {
            if (laser != anotherLaser)
            {
                return;
            }

            teleportPosition = position;
            teleportDirection = direction;

            if (teleportSpreadRoutine == null)
            {
                teleportSpreadRoutine = MonoBehaviourLifecycle.PlayCoroutine(TeleportSpreadRoutine());
            }
        }

        #endregion
    }
}
