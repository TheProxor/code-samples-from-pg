using Modules.General;
using Modules.Sound;


namespace Drawmasters.Levels
{
    public class BossHitStageLevelTargetComponent : StageLevelTargetComponent
    {
        #region Events handlers

        protected override void LevelTarget_OnHitted(LevelTarget otherLevelTarget)
        {
            base.LevelTarget_OnHitted(otherLevelTarget);

            currentStage++;

            bool isFinalShot = currentStage >= LevelStageController.StagesCount;

            if (isFinalShot)
            {
                switch (levelTarget.Type)
                {
                    case LevelTargetType.Boss:
                        SoundManager.Instance.PlayOneShot(AudioKeys.Ingame.BOSSDEATH);
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
                Scheduler.Instance.CallMethodWithDelay(this, () => levelTarget.OnHitted += LevelTarget_OnHitted, immortalityDuration);

                InvokeOnShouldChangeStageEvent(currentStage, levelTarget);
            }
        }

        #endregion
    }
}