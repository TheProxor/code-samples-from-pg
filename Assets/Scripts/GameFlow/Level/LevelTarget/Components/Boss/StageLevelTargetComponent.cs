using System;
using Modules.Sound;
using Drawmasters.Effects;
using Modules.General;


namespace Drawmasters.Levels
{
    public abstract class StageLevelTargetComponent : LevelTargetComponent
    {
        #region Fields

        public static event Action<int, LevelTarget> OnShouldChangeStage;

        protected int currentStage;        

        #endregion



        #region Overrided methods

        public override void Enable()
        {
            currentStage = default;

            levelTarget.OnHitted += LevelTarget_OnHitted;
        }


        public override void Disable()
        {
            TouchManager.Instance.IsEnabled = true;
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);

            levelTarget.OnHitted -= LevelTarget_OnHitted;            
        }

        #endregion



        #region Protected methods

        protected void PlayHitSounds()
        {
            SoundManager.Instance.PlaySound(SoundGroupKeys.RandomHitKey);
            SoundManager.Instance.PlaySound(SoundGroupKeys.RandomBossEmotionKey);
        }


        protected void PlayHitVfxes() =>
            EffectManager.Instance.PlaySystemOnce(EffectKeys.FxObjectBossCollision, (levelTarget as EnemyBossBase).CenterPosition);


        protected void InvokeOnShouldChangeStageEvent(int index, LevelTarget levelTarget) =>
            OnShouldChangeStage?.Invoke(index, levelTarget);

        #endregion



        #region Events handlers

        protected virtual void LevelTarget_OnHitted(LevelTarget levelTarget)
        {
            levelTarget.OnHitted -= LevelTarget_OnHitted;

            PlayHitSounds();
            PlayHitVfxes();
        }

        #endregion
    }
}
