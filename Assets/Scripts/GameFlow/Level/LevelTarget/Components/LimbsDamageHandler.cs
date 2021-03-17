using System;
using System.Collections.Generic;
using Modules.Sound;
using Drawmasters.Effects;
using Drawmasters.ServiceUtil;
using Modules.General;
using UnityEngine;
using Drawmasters.Statistics.Data;


namespace Drawmasters.Levels
{
    public abstract class LimbsDamageHandler : ProjectileHitHandler
    {
        #region Fields

#if UNITY_EDITOR
            private const float MinDamageForLog = 10.0f;
            private const float TimeForProjectileDamageCheckLog = 0.1f;
            private float totalCheckedDamageForLog;
            public static event Action<string> OnShouldLog;
#endif

        private bool wasLiquidPerfectSent;

        #endregion



        #region Lifecycle

        public LimbsDamageHandler(List<LevelTargetLimb> _enemyLimbs) : base(_enemyLimbs) { }

        #endregion



        #region Methods

        public override void Enable()
        {
            base.Enable();

            DynamiteExplosion.OnLimbPartExploded += DynamiteExplosion_OnLimbPartExploded;
            wasLiquidPerfectSent = false;
        }

        public override void Disable()
        {
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);

            DynamiteExplosion.OnLimbPartExploded -= DynamiteExplosion_OnLimbPartExploded;

            base.Disable();
        }


        protected override bool HandleProjectileCollision(Projectile projectile,
                                                          LevelTargetLimb limb,
                                                          out float damage)
        {
            bool isUniqueHit = base.HandleProjectileCollision(projectile,
                                                              limb,
                                                              out damage);
            if (!isUniqueHit)
            {
                return isUniqueHit;
            }

            LevelTargetSettings.LimbData foundData = levelTarget.Settings.FindLimbData(limb.RootBoneName);
            LevelTargetSettings.ProjectileData projectileData = levelTarget.Settings.FindProjectileData(projectile.Type);
            if (foundData == null ||
                projectileData == null)
            {
                return false;
            }

            float damageMultiplier = foundData.damageMultiplier;
            float baseDamageForLimb = projectileData.baseDamageForLimb;

            damage = baseDamageForLimb * damageMultiplier;

            if (!levelTarget.IsHitted && damage >= levelTarget.Settings.minDamageToDie)
            {
                if (!levelTarget.IsHitted)
                {
                    PerfectsManager.PerfectReceiveNotify(foundData.perfectTypeOnProjectileCollision, limb.transform.position, levelTarget);
                }

                // hack hot fix
                if ((levelTarget as EnemyBoss) == null)
                {
                    SoundManager.Instance.PlaySound(SoundGroupKeys.RandomHitKey);
                }
            }

            ApplyDamage(damage);

            PlayEffect(projectile.ProjectileSpriteRoot);

#if UNITY_EDITOR
                StartCheckLog(damage);
#endif

            return true;


#if UNITY_EDITOR
                void StartCheckLog(float damageRecieve)
                {
                    totalCheckedDamageForLog += damageRecieve;

                    Scheduler.Instance.UnscheduleAllMethodForTarget(this);

                    Scheduler.Instance.CallMethodWithDelay(this, () =>
                    {
                        string logText = $"{levelTarget.name} Get {totalCheckedDamageForLog} damage from PROJECTILE";
                        totalCheckedDamageForLog = 0;

                        OnShouldLog?.Invoke(logText);

                    }, TimeForProjectileDamageCheckLog);
                }
#endif
        }


        protected override void HandlePhysicalLevelObjectCollision(PhysicalLevelObject physicalLevelObject,
                                                                   LevelTargetLimb limb,
                                                                   out float damage)
        {
            damage = PhysicsCalculation.CalculateLimbReceivedDamage(physicalLevelObject,
                                                                    limb,
                                                                    levelTarget);

            bool canSendAnnouncer = !levelTarget.IsHitted ||
                                    (levelTarget.IsHitted && damage > levelTarget.Settings.hittedMinDamageForObjectDropPerfect);

            if (canSendAnnouncer)
            {
                bool wasSent = SendPerfectReceiveEvent(limb.RootBoneName, damage, PerfectType.ObjectDrop, limb.transform.position);
                if (wasSent)
                {
                    SoundManager.Instance.PlaySound(SoundGroupKeys.RandomBodyFallDownKey);
                }
            }
            

            ApplyDamage(damage);

#if UNITY_EDITOR

            float chopOffAllDamage = PhysicsCalculation.CalculateLimbReceivedDamageForChopOffAll(physicalLevelObject,
                                                                    limb,
                                                                    levelTarget);
            if (chopOffAllDamage > MinDamageForLog)
            {
                string logText = $"{levelTarget.name} Get {Math.Round(chopOffAllDamage, 1)} CHOP OFF damage from OBJECT {physicalLevelObject} with velocity {Math.Round(physicalLevelObject.Rigidbody2D.velocity.magnitude, 1)}";
                OnShouldLog?.Invoke(logText);
            }

            if (damage > MinDamageForLog)
            {
                string logText = $"{levelTarget.name} Get {Math.Round(damage, 1)} damage from OBJECT {physicalLevelObject} with velocity {Math.Round(physicalLevelObject.Rigidbody2D.velocity.magnitude, 1)}";
                OnShouldLog?.Invoke(logText);
            }
#endif
        }


        protected override void HandleSpikesCollision(Spikes spikes, LevelTargetLimb limb, out float damage)
        {
            damage = float.MaxValue;

            if (!levelTarget.CurrentEnteredSpikes.Contains(spikes) &&
                levelTarget.IsHitted &&
                Array.Exists(levelTarget.Settings.hittedBonesNamesForPerfects, e => e == limb.RootBoneName))
            {
                PerfectsManager.PerfectReceiveNotify(PerfectType.SpikesEnter, limb.transform.position, levelTarget);

                PlayerData playerData = GameServices.Instance.PlayerStatisticService.PlayerData;
                if (playerData.IsBloodEnabled)
                {
                    Vector3 effectPosition = spikes.transform.InverseTransformPoint(limb.transform.position);
                    effectPosition = effectPosition.SetY(0.0f);

                    EffectManager.Instance.PlaySystemOnce(EffectKeys.FxBloodSpikes,
                                                          effectPosition,
                                                          Quaternion.identity,
                                                          spikes.transform,
                                                          TransformMode.Local);
                }
                SoundManager.Instance.PlayOneShot(SoundGroupKeys.RandomLimbChopOffKey);

                levelTarget.CurrentEnteredSpikes.Add(spikes);
            }
        }


        protected override void HandleMonolithCollision(LevelTargetLimb limb, out float damage)
        {
            damage = PhysicsCalculation.CalculateLimbReceivedDamageFromMonolit(limb, levelTarget);

            bool canSendAnnouncer = !levelTarget.IsHitted ||
                                    (levelTarget.IsHitted && damage > levelTarget.Settings.hittedMinDamageForSkyFallPerfect);

            if (canSendAnnouncer)
            {
                PerfectType type = (damage > levelTarget.Settings.minDamageForSkyFallPerfect) ? PerfectType.SkyFall : PerfectType.Stumbling;
                bool wasSent = SendPerfectReceiveEvent(limb.RootBoneName, damage, type, limb.transform.position);
                if (wasSent)
                {
                    SoundManager.Instance.PlaySound(SoundGroupKeys.RandomBodyFallDownKey);
                }
            }
            

            ApplyDamage(damage);

            float velocityMagnitude = PhysicsCalculation.AverageLimbVelocityMagnitude(limb, levelTarget);

#if UNITY_EDITOR
                if (damage > MinDamageForLog && !levelTarget.IsHitted)
                {
                    string logText = $"{levelTarget.name} Get {Math.Round(damage, 1)} damage from MONOLITH. Limb velocity was {Math.Round(velocityMagnitude, 1)}";
                    OnShouldLog?.Invoke(logText);
                }
#endif
        }


        protected override void HandleAnotherLimbPartCollision(LevelTargetLimbPart anotherPart,
                                                               LevelTarget anotherLevelTarget,
                                                               LevelTargetLimb currentTargetLimb,
                                                               out float damage)
        {
            Rigidbody2D anotherLimbPartRigidbody = anotherLevelTarget.Ragdoll2D.GetRigidbody(anotherPart.BoneName);

            damage = PhysicsCalculation.CalculateLimbDamageFromRigidbody(currentTargetLimb,
                                                                                        levelTarget,
                                                                                        anotherLimbPartRigidbody);

            if (damage >= levelTarget.Settings.minDamageToDie)
            {
                if (!levelTarget.IsHitted)
                {
                    PerfectsManager.PerfectReceiveNotify(PerfectType.LimbKill, currentTargetLimb.transform.position, levelTarget);
                }

                SoundManager.Instance.PlaySound(SoundGroupKeys.RandomBodyFallDownKey);
            }

            ApplyDamage(damage);

            float velocityMagnitude = PhysicsCalculation.AverageLimbVelocityMagnitude(currentTargetLimb, levelTarget);

#if UNITY_EDITOR
                if (damage > MinDamageForLog)
                {
                    string logText = $"{levelTarget.name} Get {Math.Round(damage, 1)} damage from ANOTHER LIMB. Limb velocity was {Math.Round(velocityMagnitude, 1)}";
                    OnShouldLog?.Invoke(logText);
                }
#endif
        }


        protected virtual void HandleLimbExplosion(string limbName,
                                                   DynamiteSettings.ExplosionData explosionData,
                                                   out float damage)
        {
            // TODO improve logic
            damage = explosionData.enemyDamage;

            if (!levelTarget.IsHitted)
            {
                SendPerfectReceiveEvent(limbName, damage, PerfectType.Boom, levelTarget.Ragdoll2D.RootRigidbody.position);
            }

            levelTarget.MarkHitted();
        }


        protected override void HandleLiquidCollision(LiquidLevelObject liquid,
                                                      LevelTargetLimb limb,
                                                      out float damage)
        {
            damage = float.MaxValue;

            if (!wasLiquidPerfectSent)
            {
                bool wasPerfectSent = SendPerfectReceiveEvent(limb.RootBoneName, damage, PerfectType.AcidEnter, limb.transform.position);

                if (wasPerfectSent)
                {
                    wasLiquidPerfectSent = true;
                }

                if (!levelTarget.IsHitted)
                {
                    Scheduler.Instance.CallMethodWithDelay(this,
                                                           levelTarget.MarkHitted,
                                                           IngameData.Settings.liquidSettings.levelTargetDestoyDelay);
                }
            }
        }


        private void ApplyDamage(float damageToReceive)
        {
            if (damageToReceive >= levelTarget.Settings.minDamageToDie)
            {
                levelTarget.MarkHitted();
            }
        }


        private bool SendPerfectReceiveEvent(string limbRootName, float damageToReceive, PerfectType perfectType, Vector3 position)
        {
            if (damageToReceive >= levelTarget.Settings.minDamageToDie &&
                (!levelTarget.IsHitted ||
                 Array.Exists(levelTarget.Settings.hittedBonesNamesForPerfects, e => e == limbRootName)))
            {
                PerfectsManager.PerfectReceiveNotify(perfectType, position, levelTarget);
                return true;
            }

            return false;
        }


        private void PlayEffect(Transform projectile)
        {
            PlayerData playerData = GameServices.Instance.PlayerStatisticService.PlayerData;
            if (playerData.IsBloodEnabled)
            {
                Quaternion vfxRotation = Quaternion.Euler(0.0f, 0.0f, projectile.eulerAngles.z + 180.0f);

                EffectManager.Instance.PlaySystemOnce(EffectKeys.FxBloodSmallBurst,
                                                      projectile.transform.position,
                                                      vfxRotation,
                                                      null,
                                                      TransformMode.World);
            }
        }

        #endregion


        #region Event handler

        private void DynamiteExplosion_OnLimbPartExploded(LevelTarget enemy,
                                                          LevelTargetLimbPart limb,
                                                          DynamiteSettings.ExplosionData explosionData)
        {
            if (enemy == levelTarget)
            {
                string limbParent = ParentBoneName(limb.BoneName);
                if (string.IsNullOrEmpty(limbParent))
                {
                    return;
                }

                HandleLimbExplosion(limbParent, explosionData, out float damage);
            }
        }

        #endregion
    }
}
