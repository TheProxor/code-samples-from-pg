using Modules.General;
using Modules.Sound;

namespace Drawmasters.Levels
{
    public class HittedLevelTargetComponent : LevelTargetComponent
    {
        #region Fields

        private static int simultaneousDeathCount;

        #endregion



        #region Methods

        public override void Enable()
        {
            levelTarget.OnHitted += LevelTarget_OnHitted;
        }


        public override void Disable()
        {
            levelTarget.OnHitted -= LevelTarget_OnHitted;

            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
        }


        private void MarkNotSimultaneousDeaths()
        {
            simultaneousDeathCount--;
        }

        #endregion



        #region Events handlers

        private void LevelTarget_OnHitted(LevelTarget target)
        {
            if (!levelTarget.Equals(target))
            {
                CustomDebug.Log("Link are not eqauls to subscriber in HittedLevelTargetComponent.cs Something went wrong.");
                return;
            }

            if (!(levelTarget is EnemyBossBase)) // hack hot fix. boss sound logic in BossSounds.cs
            {
                CommonLevelTargetSettings levelTargetSettings = IngameData.Settings.levelTarget;

                float soundDelay = simultaneousDeathCount * levelTargetSettings.SimultaneousDeathSoundDelay;
                Scheduler.Instance.CallMethodWithDelay(this, () =>
                {
                    SoundManager.Instance.PlaySound(SoundGroupKeys.RandomCharacterEmotionKey);
                }, soundDelay);

                simultaneousDeathCount++;
                Scheduler.Instance.CallMethodWithDelay(this, MarkNotSimultaneousDeaths, levelTargetSettings.SimultaneousDeathDuration);
            }

            levelTarget.ApplyRagdoll();
        }

        #endregion
    }
}
