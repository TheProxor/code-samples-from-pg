using Drawmasters.ServiceUtil;
using Drawmasters.Utils;
using Modules.Sound;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class ProjectilePullThrowComponent : ProjectileShotComponent
    {
        #region Fields

        public static event Action<LevelObject, Rigidbody2D> OnObjectPull;
        public static event Action<LevelObject> OnObjectReleased;

        public static event Action OnPullMiss;
        public static event Action OnHoldBreak;

        private static readonly int raycastMask = ~LayerMask.GetMask(PhysicsLayers.Shooter);

        private static readonly List<CollidableObjectType> AllowPulledTypes = new List<CollidableObjectType> { CollidableObjectType.EnemyStand,
                                                                                                               CollidableObjectType.EnemyTrigger,
                                                                                                               CollidableObjectType.PhysicalObject,
                                                                                                               CollidableObjectType.Spikes };

        private static readonly List<PhysicalLevelObjectType> ExceptedPhysicalLevelObjectTypes = new List<PhysicalLevelObjectType> { PhysicalLevelObjectType.Monolit };


        private static readonly bool EnableLogs = false;

        private HitmastersGravitygunSettings gravitySettings;

        private Transform projectileSpawnRoot;

        private float hitDistance;
        private float throwImpulsMagnitude;

        private float previuosPulledRigidbodyGravity;
        private float previousHoldAngle;
        private Vector3 previousDirection;

        private LevelObject pulledLevelObject;
        private Rigidbody2D pulledRigidbody2D;

        private Coroutine holdRigidbodyRoutine;

        private Guid holdSoundGuid;
        
        #endregion



        #region Properties

        private Vector3 WeaponDirection
        {
            get
            {
                int directionMultiplier = projectileSpawnRoot.eulerAngles.y < 180.0f ? 1 : -1;
                return Quaternion.Euler(0.0f, 0.0f, projectileSpawnRoot.eulerAngles.z) * (directionMultiplier * Vector3.right);
            }
        }


        private Vector3 PulledTargetPosition => projectileSpawnRoot.position + WeaponDirection * gravitySettings.distanceToStopPull;


        #endregion



        #region Methods

        public override void Initialize(Projectile mainProjectileValue, WeaponType mode)
        {
            base.Initialize(mainProjectileValue, mode);

            GravitygunWeapon.OnShouldPullObject += GravityWeapon_OnShouldPullObject;
            GravitygunWeapon.OnShouldThrowObject += GravityWeapon_OnShouldThrowObject;
            Level.OnLevelStateChanged += Level_OnLevelStateChanged;

            WeaponSkinType weaponSkinType = GameServices.Instance.PlayerStatisticService.PlayerData.GetCurrentWeaponSkin(mode);
        }


        public override void Deinitialize()
        {
            pulledRigidbody2D = null;
            mainProjectile.StopAllCoroutines();

            GravitygunWeapon.OnShouldPullObject -= GravityWeapon_OnShouldPullObject;
            GravitygunWeapon.OnShouldThrowObject -= GravityWeapon_OnShouldThrowObject;
            Level.OnLevelStateChanged -= Level_OnLevelStateChanged;

            MonoBehaviourLifecycle.StopPlayingCorotine(holdRigidbodyRoutine);
            SoundManager.Instance.StopSound(holdSoundGuid);

            base.Deinitialize();
        }


        protected override void ApplySettings(WeaponSettings settings)
        {
            switch (settings)
            {
                case HitmastersGravitygunSettings gravitygunModeSettings:
                    gravitySettings = gravitygunModeSettings;
                    break;

                default:
                    CustomDebug.Log($"Apply logic for settings {settings} not implemented in {this}!");
                    break;
            }
        }


        private void ThrowPulledObject(Vector2 directionVector)
        {
            MonoBehaviourLifecycle.StopPlayingCorotine(holdRigidbodyRoutine);

            float pulledDistance = hitDistance - Vector2.Distance(pulledRigidbody2D.position, PulledTargetPosition);
            float distanceFactor = hitDistance >= 0.0f ? pulledDistance / hitDistance : 0.0f;

            bool isEnoguhtDistanceForThrow = distanceFactor >= gravitySettings.minDistanceFactorForThrowHit ||
                                             Mathf.Approximately(hitDistance, 0.0f);

            if (EnableLogs)
            {
                CustomDebug.Log($"isEnoguhtDistanceForThrow {isEnoguhtDistanceForThrow}");
                CustomDebug.Log($"hitDistance {hitDistance}");
                CustomDebug.Log($"pulledDistance {pulledDistance}");
                CustomDebug.Log($"distanceFactor {distanceFactor}");
            }

            ThrowOutPulledObject(directionVector, isEnoguhtDistanceForThrow);
        }


        private void ThrowOutPulledObject(Vector2 directionVector, bool isEnoughDistanceForThrow)
        {
            if (isEnoughDistanceForThrow)
            {
                Rigidbody2D savedRigidBody = pulledRigidbody2D;
                gravitySettings.gravityReturnAnimation.Play((value) =>
                {
                    if (savedRigidBody != null)
                    {
                        savedRigidBody.gravityScale = previuosPulledRigidbodyGravity * value;
                    }
                }, savedRigidBody);
            }
            else
            {
                pulledRigidbody2D.gravityScale = previuosPulledRigidbodyGravity;
            }

            float magnitude = isEnoughDistanceForThrow ? (throwImpulsMagnitude) : gravitySettings.fallImpulsMagnitude;
            pulledRigidbody2D.velocity = magnitude * directionVector.normalized;

            SoundManager.Instance.PlayOneShot(AudioKeys.Ingame.GRAVYGUNSHOOTOUT);

            OnObjectReleased?.Invoke(pulledLevelObject);
        }


        private void PullFirstObject(Vector3 position, Vector2 directionVector)
        {
            RaycastHit2D[] hits = Physics2D.CircleCastAll(position, gravitySettings.shotRendererWidth * 0.5f, directionVector, float.MaxValue, raycastMask);

            foreach (var hit in hits)
            {
                if (hit.collider != null)
                {
                    CollidableObject collidableObject = hit.transform.GetComponent<CollidableObject>();

                    if (collidableObject != null)
                    {
                        if (!AllowPulledTypes.Contains(collidableObject.Type))
                        {
                            break;
                        }

                        float pullSpeedMultiplier = 1.0f;

                        if (collidableObject.PhysicalLevelObject != null)
                        {
                            if (ExceptedPhysicalLevelObjectTypes.Contains(collidableObject.PhysicalLevelObject.PhysicalData.type))
                            {
                                continue;
                            }

                            if (collidableObject.PhysicalLevelObject.Rigidbody2D.bodyType == RigidbodyType2D.Static ||
                                collidableObject.PhysicalLevelObject.Rigidbody2D.constraints == RigidbodyConstraints2D.FreezeAll)
                            {
                                break;
                            }

                            collidableObject.PhysicalLevelObject.StopMoving();
                            collidableObject.PhysicalLevelObject.OnPreDestroy += PhysicalLevelObject_OnPreDestroy;

                            pulledLevelObject = collidableObject.PhysicalLevelObject;
                            pulledRigidbody2D = pulledLevelObject.Rigidbody2D;

                            throwImpulsMagnitude = gravitySettings.throwImpulsMagnitudeObjects;
                        }
                        else if (collidableObject.LevelTarget != null)
                        {
                            LevelTarget pulledLevelTarget = collidableObject.LevelTarget;

                            pulledLevelTarget.ApplyRagdoll();

                            LevelTargetLimbPart collidedLimbPart = collidableObject.GetComponent<LevelTargetLimbPart>();

                            bool isChoppedOffLimb = (collidedLimbPart == null) ? false : pulledLevelTarget.IsChoppedOffLimb(collidedLimbPart.BoneName);
                            bool isHittedAllowedLimb = (collidedLimbPart == null) ? false : gravitySettings.AllowPullBodyLimb(collidedLimbPart.BoneName);

                            bool shouldPullRootRb = (isHittedAllowedLimb && !isChoppedOffLimb) ||
                                                    (collidedLimbPart == null || !isChoppedOffLimb);

                            bool shouldPullHittedRb = (isHittedAllowedLimb && isChoppedOffLimb);

                            bool canPullRb = shouldPullRootRb || shouldPullHittedRb;

                            if (shouldPullHittedRb)
                            {
                                pulledRigidbody2D = CollisionUtility.FindLevelTargetRigidbody(collidableObject);
                                pullSpeedMultiplier = gravitySettings.LimbPullMultiplierMagnitude(collidedLimbPart.BoneName);
                            }
                            else if (shouldPullRootRb)
                            {
                                pulledRigidbody2D = pulledLevelTarget.Ragdoll2D.RootRigidbody;
                                pullSpeedMultiplier = gravitySettings.rootCharacterPullMultiplier;
                            }

                            if (canPullRb)
                            {
                                pulledLevelObject = collidableObject.LevelTarget;

                                Rigidbody2D[] ragdollRbs = pulledLevelTarget.Ragdoll2D.RigidbodyArray;

                                foreach (var rb in ragdollRbs)
                                {
                                    rb.bodyType = RigidbodyType2D.Dynamic;
                                    rb.drag = 0.0f;
                                    rb.angularDrag = 0.0f;
                                }
                            }

                            throwImpulsMagnitude = isChoppedOffLimb ? gravitySettings.throwImpulsMagnitudeLimbs : gravitySettings.throwImpulsMagnitudeLevelTarget;
                        }

                        if (pulledRigidbody2D != null)
                        {
                            // for moving objects
                            pulledRigidbody2D.bodyType = RigidbodyType2D.Dynamic;

                            float mass = pulledRigidbody2D.mass;
                            float velocity = (mass > 0.0f) ? (gravitySettings.pullSpeedForOneMassUnit * pullSpeedMultiplier / mass) : 0.0f;

                            mainProjectile.StartCoroutine(PullRigidbody(velocity));
                            hitDistance = hit.distance;

                            OnObjectPull?.Invoke(pulledLevelObject, pulledRigidbody2D);

                            return;
                        }
                    }
                }
            }

            MarkMissed();

            SoundManager.Instance.PlayOneShot(AudioKeys.Ingame.GRAVYGUNDRYSHOT);
        }


        private IEnumerator PullRigidbody(float velocity)
        {
            pulledRigidbody2D.velocity = Vector2.zero;
            pulledRigidbody2D.angularVelocity = 0.0f;
            previuosPulledRigidbodyGravity = pulledRigidbody2D.gravityScale;

            previousDirection = WeaponDirection;

            SoundManager.Instance.PlayOneShot(AudioKeys.Ingame.GRAVYGUNTARGETSHOOTIN);

            while (true)
            {
                if (pulledRigidbody2D == null)
                {
                    yield break;
                }

                Vector2 directionVector = PulledTargetPosition.ToVector2() - pulledRigidbody2D.position;

                float angleDelta = Vector2.Angle(WeaponDirection, previousDirection);
                previousDirection = WeaponDirection;

                Vector3 direction = (pulledRigidbody2D.position.ToVector3() - projectileSpawnRoot.position);
                float angle = Vector2.Angle(WeaponDirection, direction);

                bool isBehindWeapon = Vector2.Dot(WeaponDirection, direction) > 0;

                if (angle > gravitySettings.maxObjectsAngleToReset &&
                    isBehindWeapon &&
                    directionVector.magnitude > gravitySettings.maxObjectsDistanceToReset)
                {
                    ThrowOutPulledObject(directionVector, false);
                    MarkMissed();
                    OnHoldBreak?.Invoke();
                    yield break;
                }

                pulledRigidbody2D.velocity = directionVector.normalized * velocity;

                if (angleDelta > gravitySettings.anglePerFrameToStopPull)
                {
                    ThrowOutPulledObject(directionVector, false);
                    MarkMissed();
                    OnHoldBreak?.Invoke();
                    yield break;
                }

                if (directionVector.magnitude < 1.0f)
                {
                    break;
                }

                yield return new WaitForFixedUpdate();
            }

            pulledRigidbody2D.position = PulledTargetPosition;

            pulledRigidbody2D.velocity = Vector2.zero;
            pulledRigidbody2D.gravityScale = 0.0f;

            MonoBehaviourLifecycle.StopPlayingCorotine(holdRigidbodyRoutine);
            holdRigidbodyRoutine = MonoBehaviourLifecycle.PlayCoroutine(HoldObject());


            yield return new WaitForFixedUpdate();
        }


        private void MarkMissed()
        {
            OnPullMiss?.Invoke();
            OnObjectReleased?.Invoke(pulledLevelObject);

            mainProjectile.Destroy();
        }


        private IEnumerator HoldObject()
        {            
            holdSoundGuid = SoundManager.Instance.PlaySound(AudioKeys.Ingame.GRAVYGUNTARGETCLOUDLOOP, isLooping: true);

            previousHoldAngle = float.MaxValue;

            Rigidbody2D[] ragdollRBs = (pulledLevelObject is LevelTarget levelTarget) ?
                                        levelTarget.Ragdoll2D.RigidbodyArray : Array.Empty<Rigidbody2D>();

            while (true)
            {
                pulledRigidbody2D.angularVelocity = 0.0f;

                Vector3 weaponDir = WeaponDirection;
                pulledRigidbody2D.MovePosition(PulledTargetPosition);

                yield return new WaitForFixedUpdate();

                float angle = Vector2.Angle((pulledRigidbody2D.position.ToVector3() - projectileSpawnRoot.position), weaponDir);

                if (angle > gravitySettings.anglePerFrameToReset)
                {
                    foreach (var i in ragdollRBs)
                    {
                        if (i != null)
                        {
                            i.MovePosition(PulledTargetPosition);
                        }
                    }

                    if (angle > previousHoldAngle)
                    {
                        pulledRigidbody2D.gravityScale = previuosPulledRigidbodyGravity;

                        if (pulledLevelObject is PhysicalLevelObject physicalLevelObject)
                        {
                            physicalLevelObject.OnPreDestroy -= PhysicalLevelObject_OnPreDestroy;
                        }

                        SoundManager.Instance.StopSound(holdSoundGuid);

                        MarkMissed();
                        OnHoldBreak?.Invoke();
                        yield break;
                    }

                    previousHoldAngle = angle;
                }
                else
                {
                    previousHoldAngle = float.MaxValue;
                }
            }
        }

        #endregion



        #region Events handlers

        private void GravityWeapon_OnShouldThrowObject(Vector2 direction)
        {
            ThrowPulledObject(direction);
            mainProjectile.Destroy();
        }


        private void GravityWeapon_OnShouldPullObject(Transform _projectileSpawnRoot, Vector2 direction)
        {
            projectileSpawnRoot = _projectileSpawnRoot;

            PullFirstObject(projectileSpawnRoot.position, direction);
        }


        private void Level_OnLevelStateChanged(LevelState state)
        {
            switch (state)
            {
                case LevelState.FriendlyDeath:
                case LevelState.AllTargetsHitted:
                    ThrowOutPulledObject(WeaponDirection, false);
                    mainProjectile.Destroy();
                    break;

                default:
                    break;
            }
        }


        private void PhysicalLevelObject_OnPreDestroy(PhysicalLevelObject otherObject)
        {
            if (otherObject.Rigidbody2D == pulledRigidbody2D)
            {
                otherObject.OnPreDestroy -= PhysicalLevelObject_OnPreDestroy;

                MarkMissed();
            }
        }


        protected override void OnTrajectoryShot(Vector2[] trajectory) { }

        #endregion
    }
}
