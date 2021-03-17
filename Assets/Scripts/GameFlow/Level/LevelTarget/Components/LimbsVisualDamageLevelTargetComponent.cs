using UnityEngine;
using System.Collections.Generic;
using Spine;
using System.Collections;
using Modules.Sound;
using System;
using Drawmasters.Effects;
using Drawmasters.Statistics.Data;
using Drawmasters.ServiceUtil;

namespace Drawmasters.Levels
{
    public class LimbsVisualDamageLevelTargetComponent : LimbsDamageHandler
    {
        #region Fields

        public static event Action<LevelTargetLimb, float> OnMonolithCollision;

        public static bool isDismemberEnabled = true;

        #endregion



        #region Lifecycle

        public LimbsVisualDamageLevelTargetComponent(List<LevelTargetLimb> _enemyLimbs) :
            base(_enemyLimbs)
        { }

        #endregion



        #region Overrided methods

        public override void Initialize(LevelTarget _levelTarget)
        {
            base.Initialize(_levelTarget);

            if (levelTarget is Shooter shooter)
            {
                shooter.SkeletonAnimation.Skeleton.SetSlotAttachmentsToSetupPose();
            }
            else
            {
                levelTarget.SkeletonAnimation.Skeleton.SetSlotAttachmentsToSetupPose();
            }

        }


        public override void Enable()
        {
            base.Enable();

            ProjectileExplodeApplyComponent.OnShouldChopOffLimb += ProjectileExplodeApplyComponent_OnShouldChopOffLimb;
        }

        public override void Disable()
        {
            ProjectileExplodeApplyComponent.OnShouldChopOffLimb -= ProjectileExplodeApplyComponent_OnShouldChopOffLimb;

            base.Disable();
        }


        protected override void HandleLimbExplosion(string limbName,
                                                    DynamiteSettings.ExplosionData explosionData,
                                                    out float damage)
        {
            base.HandleLimbExplosion(limbName, explosionData, out damage);

            VisualizeDamage(damage, limbName);
        }


        protected override bool HandleProjectileCollision(Projectile projectile,
                                                          LevelTargetLimb limb,
                                                          out float damage)
        {
            bool canHandleCollision = base.HandleProjectileCollision(projectile,
                                                                     limb,
                                                                     out damage);
            if (!canHandleCollision)
            {
                return canHandleCollision;
            }

            VisualizeDamage(damage, limb.RootBoneName);

            return canHandleCollision;
        }


        protected override void HandlePhysicalLevelObjectCollision(PhysicalLevelObject physicalLevelObject,
                                                                   LevelTargetLimb limb,
                                                                   out float damage)
        {
            base.HandlePhysicalLevelObjectCollision(physicalLevelObject,
                                                    limb,
                                                    out damage);

            float chopoffAllDamage = PhysicsCalculation.CalculateLimbReceivedDamageForChopOffAll(physicalLevelObject,
                                                                    limb,
                                                                    levelTarget);
            ChopOffAllLimbs(chopoffAllDamage, limb.RootBoneName);

            VisualizeDamage(damage, limb.RootBoneName);
        }


        protected override void HandleMonolithCollision(LevelTargetLimb limb, out float damage)
        {
            base.HandleMonolithCollision(limb, out damage);

            VisualizeDamage(damage, limb.RootBoneName);

            OnMonolithCollision?.Invoke(limb, damage);
        }


        protected override void HandleAnotherLimbPartCollision(LevelTargetLimbPart anotherPart,
                                                               LevelTarget anotherLevelTarget,
                                                               LevelTargetLimb currentTargetLimb,
                                                               out float damage)
        {
            base.HandleAnotherLimbPartCollision(anotherPart,
                                                anotherLevelTarget,
                                                currentTargetLimb,
                                                out damage);

            VisualizeDamage(damage, currentTargetLimb.RootBoneName);
        }

        #endregion



        #region Methods

        private void VisualizeDamage(float damageToReceive, string limbRootBoneName)
        {
            if (!levelTarget.AllowVisualizeDamage)
            {
                return;
            }

            LevelTargetSettings.LimbData foundData = levelTarget.Settings.FindLimbData(limbRootBoneName);
            if (foundData == null)
            {
                return;
            }

            if (damageToReceive < IngameData.Settings.levelTarget.physicalsObjectsImpulsToApplyRagdoll)
            {
                return;
            }

            ApplyDecals(limbRootBoneName);

            if (!isDismemberEnabled)
            {
                return;
            }

            float damageToChopOff = foundData.damageToChopOff;
            float damageToExplode = foundData.damageToExplode;

            if (damageToReceive < damageToChopOff)
            {
                return;
            }
            else if (damageToReceive >= damageToChopOff &&
                     damageToReceive < damageToExplode)
            {
                ChopOffLimb(limbRootBoneName);
            }
            else
            {
                IEnumerator ExplodeDelay()
                {
                    PlayerData playerData = GameServices.Instance.PlayerStatisticService.PlayerData;
                    yield return new WaitForFixedUpdate();

                    LevelTargetSettings.LimbData limbData = IngameData.Settings.levelTargetSettings.FindLimbData(limbRootBoneName);
                    Rigidbody2D foundRigidbody2D = levelTarget.Ragdoll2D.GetRigidbody(limbRootBoneName);

                    if (foundRigidbody2D != null &&
                        limbData != null &&
                        playerData.IsBloodEnabled)
                    {
                        EffectManager.Instance.PlaySystemOnce(limbData.explodeLimbKey,
                                                              foundRigidbody2D.transform.position,
                                                              foundRigidbody2D.transform.rotation,
                                                              levelTarget.Ragdoll2D.RootRigidbody.transform);
                    }

                    ExplosionUtility.ExplodeLimb(limbRootBoneName, levelTarget);
                }

                MonoBehaviourLifecycle.PlayCoroutine(ExplodeDelay());
            }
        }


        private void ApplyDecals(string rootBoneName)
        {
            LevelTargetLimb decalLimb = levelTarget.Limbs.Find(limb =>
                limb.RootBoneName.Equals(rootBoneName));

            if (decalLimb == null)
            {
                return;
            }

            decalLimb.ApplyDecals(levelTarget.SkeletonAnimation);
        }


        private void ChopOffLimb(string boneName)
        {
            LevelTargetLimb limb = levelTarget.Limbs.Find(l => l.RootBoneName.Equals(boneName));
            if (limb == null)
            {
                return;
            }

            if (levelTarget.IsChoppedOffLimb(boneName))
            {
                return;
            }

            levelTarget.AddChoppedOffLimb(boneName);

            limb.ChopOff(levelTarget.Ragdoll2D);

            SoundManager.Instance.PlaySound(SoundGroupKeys.RandomLimbChopOffKey);
        }


        private void ChopOffAllLimbs(float chopOffDamage, string limbRootBoneName)
        {
            if (!isDismemberEnabled)
            {
                return;
            }

            LevelTargetSettings.LimbData foundData = levelTarget.Settings.FindLimbData(limbRootBoneName);
            if (foundData == null)
            {
                return;
            }

            if (chopOffDamage >= foundData.damageToChopOffAllLimbs)
            {
                foreach (var limb in levelTarget.Limbs)
                {
                    if (!levelTarget.IsChoppedOffLimb(limb.RootBoneName))
                    {
                        ChopOffLimb(limb.RootBoneName);
                    }
                }
            }
        }

        #endregion



        #region Events handlers

        private void ProjectileExplodeApplyComponent_OnShouldChopOffLimb(LevelTarget anotherLevelTarget, float damage, string boneName)
        {
            if (levelTarget != anotherLevelTarget)
            {
                return;
            }

            ChopOffAllLimbs(damage, boneName);
        }

        #endregion
    }
}
