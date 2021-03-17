using Modules.General;

namespace Drawmasters.Levels
{
    public class BossAttackStageLevelTargetComponent : StageLevelTargetComponent
    {
        #region Fields

        private bool isRocketsForStageDestroyed;

        #endregion



        #region Overrided methods

        public override void Enable()
        {
            base.Enable();

            LevelTargetRocketLaunchComponent.OnAllRocketsDestroyed += LevelTargetRocketLaunchComponent_OnAllRocketesDestroyed;
            Level.OnLevelStateChanged += Level_OnLevelStateChanged;            
        }


        public override void Disable()
        {
            base.Disable();
            
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);

            Level.OnLevelStateChanged -= Level_OnLevelStateChanged;
            LevelTargetRocketLaunchComponent.OnAllRocketsDestroyed -= LevelTargetRocketLaunchComponent_OnAllRocketesDestroyed;            
            LevelTargetRocketLaunchComponent.OnAllRocketsDestroyed -= ChangeStage;
        }

        #endregion



        #region Private methods

        private void ChangeStage()
        {
            LevelTargetRocketLaunchComponent.OnAllRocketsDestroyed -= ChangeStage;

            currentStage++;

            bool isFinalShot = currentStage >= LevelStageController.StagesCount;

            if (isFinalShot)
            {
                switch (levelTarget.Type)
                {
                    case LevelTargetType.Boss:
                        (levelTarget as EnemyBossBase).MarkDefeated();
                        break;

                    default:
                        CustomDebug.Log($"No logic for enemy {levelTarget.Type} in {this}");
                        break;
                }
            }
            else
            {
                float immortalityDuration = IngameData.Settings.bossLevelSettings.delayBetweenStages;
                Scheduler.Instance.CallMethodWithDelay(this, () =>
                {
                    levelTarget.OnHitted += LevelTarget_OnHitted;
                    isRocketsForStageDestroyed = false;
                }, immortalityDuration);

                InvokeOnShouldChangeStageEvent(currentStage, levelTarget);
            }
        }

        #endregion



        #region Events handlers

        private void Level_OnLevelStateChanged(LevelState state)
        {
            if (state == LevelState.ReturnToInitial ||
                state == LevelState.FinishDrawing ||
                state == LevelState.FriendlyDeath)
            {
                isRocketsForStageDestroyed = false;
                Scheduler.Instance.UnscheduleAllMethodForTarget(this);
                LevelTargetRocketLaunchComponent.OnAllRocketsDestroyed -= ChangeStage;
            }

            if (state == LevelState.ReturnToInitial)
            {
                levelTarget.OnHitted -= LevelTarget_OnHitted;
                levelTarget.OnHitted += LevelTarget_OnHitted;
            }

            if (state == LevelState.FriendlyDeath)
            {
                levelTarget.OnHitted -= LevelTarget_OnHitted;
            }
        }


        protected override void LevelTarget_OnHitted(LevelTarget levelTarget)
        {
            base.LevelTarget_OnHitted(levelTarget);
            
            if (isRocketsForStageDestroyed)
            {
                ChangeStage();
            }
            else
            {
                LevelTargetRocketLaunchComponent.OnAllRocketsDestroyed += ChangeStage;
            }
        }


        private void LevelTargetRocketLaunchComponent_OnAllRocketesDestroyed()
        {
            isRocketsForStageDestroyed = true;
        }

        #endregion
    }
}
