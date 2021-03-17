using Drawmasters.Effects;
using Modules.General;
using Modules.Sound;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class EnemyHitmastersBoss : EnemyBossGeneric<BossSerializableData>
    {
        #region Fields

        private Vector3 nextStageTargetPosition;
        object colorTweenId;

        #endregion



        #region Abstract implementation

        protected override float ObjectsFreeFallDelay
            //TODO change value
            => IngameData.Settings.bossLevelSettings.objectFreeFallDelay;

        protected override string SkinWithBoundingBoxesName            
            => IngameData.Settings.bossLevelTargetSettings.hitmastersBossSkinWithoutBoundingBoxes;

        protected override List<string> AvailableSkins
            => IngameData.Settings.bossLevelTargetSettings.hitmastersBossSkins.ToList();

        protected override List<LevelTargetComponent> CreateComponents()
        {
            List<LevelTargetComponent> result = new List<LevelTargetComponent>()
            {
                new StandLevelTargetComponent(standColliders),
                new HittedLevelTargetComponent(),
                new LimbPartsImpulsLevelTargetComponent(limbsParts),
                new LimbsLiquidLevelTargetComponent(limbs),
                //new LimbsVisualDamageLevelTargetComponent(limbs),
                new LimbsVisualDamageHitmastersLevelsLevelTarget(limbs),
                //new LimbsAcidLevelTargetComponent(limbs),
                new FixedJointLevelTargetComponent(),
                new ImpulsRagdollApplyLevelTargetComponent(limbs),
                new RagdollEffectsComponent(),
                new BossSounds(),
                new LevelTargetImmortalityComponent(),
                new StageHealthBarLevelTargetComponent(healthBar),
                new RagdollLevelTargetComponent(),
                new LimbsLaserLevelTargetComponent(),
                new BossAnimationBase(skeletonAnimation,
                                  IngameData.Settings.levelTargetAnimationNamesSettings.hitmastersBossAnimationNames), // TODO change values

                                  // overrides
                new BossHitStageLevelTargetComponent(),
            };

            return result;        
        }

        protected override void OnStateChanged()
            => ChangeStage();

        #endregion



        #region Overrided methods

        public override void ApplyRagdoll()
        {
            if (!isDefeated)
            {
                return;
            }

            base.ApplyRagdoll();
        }

        public override void StartStageChange(StageLevelObjectData data, int stage)
        {
            base.StartStageChange(data, stage);

            bool isFirstStage = stage == 0;

            nextStageTargetPosition = data.position;

            CameraShakeSettings.Shake shake = IngameData.Settings.cameraShakeSettings.bossStageChange;
            IngameCamera.Instance.Shake(shake);

            if (StandRigidbody.bodyType != RigidbodyType2D.Static)
            {
                StandRigidbody.velocity = default;
                StandRigidbody.angularVelocity = default;
            }

            StandRigidbody.bodyType = RigidbodyType2D.Kinematic;

            CommonUtility.SetObjectActive(stageChangeRoot, true);
            CommonUtility.SetObjectActive(limbsRoot, false);

            if (isFirstStage)
            {
                SetSlotsColor(Color.clear);
            }

            colorTweenId = Guid.NewGuid();
            BossLevelTargetSettings bossLevelTargetSettings = IngameData.Settings.bossLevelTargetSettings;

            if (!isFirstStage)
            {
                float fadeOutEffectDelay = IngameData.Settings.bossLevelSettings.fadeOutBossEffectdelay;
                Scheduler.Instance.CallMethodWithDelay(this, () =>
                {
                    EffectManager.Instance.PlaySystemOnce(EffectKeys.FxTeleportFade, transform.position);
                }, fadeOutEffectDelay);

                bossLevelTargetSettings.bossFadeAnimation.Play(SetSlotsColor, colorTweenId);
            }

            float appearEffectDelay = IngameData.Settings.bossLevelSettings.bossAppearEffectDelay;
            string fxKey = isFirstStage ? EffectKeys.FxTeleportBossFirstAppear : EffectKeys.FxTeleportAppear;
            Scheduler.Instance.CallMethodWithDelay(this, () =>
            {
                EffectManager.Instance.PlaySystemOnce(fxKey, nextStageTargetPosition);
                SoundManager.Instance.PlayOneShot(AudioKeys.Ingame.PORTAL);
            }, appearEffectDelay);

            bossLevelTargetSettings.bossAppearAnimation.Play(SetSlotsColor,
                                                             colorTweenId,
                                                             InvokeOnAppearEvent);
        }


        public override void FinishStageChange(int stage)
        {
            CommonUtility.SetObjectActive(stageChangeRoot, false);
            CommonUtility.SetObjectActive(limbsRoot, true);

            StandRigidbody.bodyType = RigidbodyType2D.Dynamic;

            base.FinishStageChange(stage);
        }

        #endregion



        #region Private methods

        private void ChangeStage()
        {
            transform.position = nextStageTargetPosition;
            transform.eulerAngles = currentStageData.rotation;

            Physics2D.SyncTransforms();
        }


        #endregion
    }
}

