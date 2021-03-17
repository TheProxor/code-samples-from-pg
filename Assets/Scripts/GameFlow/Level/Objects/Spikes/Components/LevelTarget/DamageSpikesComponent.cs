using Modules.Sound;
using Drawmasters.Effects;
using UnityEngine;
using Drawmasters.Statistics.Data;
using Drawmasters.ServiceUtil;


namespace Drawmasters.Levels
{
    public class DamageSpikesComponent : LevelTargetCollisionSpikesComponent
    {
        #region Fields

        private bool isLevelStageChanging;

        private LevelStageController stageController;

        #endregion



        #region Methods

        public override void Initialize(CollisionNotifier notifier, Rigidbody2D rigidbody, PhysicalLevelObject sourceObject)
        {
            base.Initialize(notifier, rigidbody, sourceObject);

            stageController = GameServices.Instance.LevelControllerService.Stage;

            // TODO editor check?
            if (LevelsManager.HasFoundInstance)
            {
                stageController.OnStartChangeStage += LevelStageController_OnStartChangeStage;
                stageController.OnFinishChangeStage += LevelStageController_OnFinishChangeStage;
            }

            isLevelStageChanging = false;
        }


        public override void Disable()
        {
            if (LevelsManager.HasFoundInstance)
            {
                stageController.OnStartChangeStage -= LevelStageController_OnStartChangeStage;
                stageController.OnFinishChangeStage -= LevelStageController_OnFinishChangeStage;
            }

            base.Disable();
        }


        protected override void OnLevelTargetCollision(LevelTarget levelTarget)
        {
            base.OnLevelTargetCollision(levelTarget);

            bool allowDamageBoss = levelTarget.Type == LevelTargetType.Boss && !isLevelStageChanging && !levelTarget.IsHitted;
            bool allowDamageTarget = levelTarget.Type != LevelTargetType.Boss && !levelTarget.IsHitted;

            if (allowDamageTarget || allowDamageBoss)
            {
                levelTarget.ApplyRagdoll();
                levelTarget.MarkHitted();
                
                PerfectsManager.PerfectReceiveNotify(PerfectType.SpikesEnter, levelTarget.transform.position, levelTarget);

                if (sourceLevelObject is Spikes spikes)
                {
                    levelTarget.CurrentEnteredSpikes.Add(spikes);
                }

                PlayerData playerData = GameServices.Instance.PlayerStatisticService.PlayerData;
                if (playerData.IsBloodEnabled)
                {
                    Vector3 levelTargetPosition = levelTarget.Ragdoll2D.IsActive ?
                        levelTarget.Ragdoll2D.EstimatedSkeletonPosition : levelTarget.transform.position;
                    Vector3 effectPosition = sourceLevelObject.transform.InverseTransformPoint(levelTargetPosition);
                    effectPosition = effectPosition.SetY(0.0f);

                    EffectManager.Instance.PlaySystemOnce(EffectKeys.FxBloodSpikes,
                                                          effectPosition,
                                                          Quaternion.identity,
                                                          sourceLevelObject.transform,
                                                          TransformMode.Local);
                }


                SoundManager.Instance.PlaySound(SoundGroupKeys.RandomLimbChopOffKey);
            }
        }

        #endregion


        #region Events handlers

        private void LevelStageController_OnStartChangeStage() =>
            isLevelStageChanging = true;


        private void LevelStageController_OnFinishChangeStage() =>
            isLevelStageChanging = false;

        #endregion
    }
}
