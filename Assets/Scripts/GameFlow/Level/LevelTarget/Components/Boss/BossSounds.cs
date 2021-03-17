using Modules.General;
using Modules.Sound;


namespace Drawmasters.Levels
{
    public class BossSounds : EnemySounds
    {
        #region Methods

        public override void Enable()
        {
            base.Enable();

            if (levelTarget is EnemyBoss enemyBoss)
            {
                enemyBoss.OnAppeared += EnemyBoss_OnAppeared;
                enemyBoss.OnDefeated += EnemyBoss_OnDefeated;
            }

            Scheduler.Instance.CallMethodWithDelay(this,
                () => SoundManager.Instance.PlayOneShot(AudioKeys.Ingame.BOSSLEVELSTART),
                IngameData.Settings.bossLevelSettings.soundGreetingDelay);
        }


        public override void Disable()
        {
            if (levelTarget is EnemyBoss enemyBoss)
            {
                enemyBoss.OnAppeared -= EnemyBoss_OnAppeared;
                enemyBoss.OnDefeated -= EnemyBoss_OnDefeated;
            }

            Scheduler.Instance.UnscheduleAllMethodForTarget(this);

            base.Disable();
        }


        private void PlaySound(string sound, float delay) =>
            Scheduler.Instance.CallMethodWithDelay(this, () => SoundManager.Instance.PlayOneShot(sound), delay);
        
        #endregion



        #region Events handlers

        private void EnemyBoss_OnAppeared()
        {
            string laughterKey = SoundGroupKeys.GetBossLaughterKey(LevelStageController.CurrentStageIndex);
            SoundManager.Instance.PlayOneShot(laughterKey);
        }


        private void EnemyBoss_OnDefeated(LevelTarget defeatedLevelTarget)
        {
            BossLevelTargetSettings settings = IngameData.Settings.bossLevelTargetSettings;

            PlaySound(AudioKeys.Ingame.BOSSDEATH, settings.defeatSoundDelay);

            foreach (var delay in settings.defeatExplodeSoundsDelay)
            {
                PlaySound(AudioKeys.Ingame.EXPLODE_1, delay);
            }
        }

        #endregion
    }
}
