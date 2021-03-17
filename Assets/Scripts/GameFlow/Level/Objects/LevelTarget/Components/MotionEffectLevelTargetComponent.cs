using System;


namespace Drawmasters.Levels
{
    public class MotionEffectLevelTargetComponent : LevelTargetComponent
    {
        #region Fields

        public static event Action<LevelTarget> OnShouldPlayEffect;

        #endregion



        #region Methods

        public override void Enable()
        {
            levelTarget.OnDefeated += LevelTarget_OnDefeated;
            BossLevelEndEffectComponent.OnShouldPlayEffects += BossLevelEndEffectComponent_OnShouldPlayEffects;
        }


        public override void Disable() =>
            StopCheckingConditions();
        


        private void StopCheckingConditions()
        {
            levelTarget.OnDefeated -= LevelTarget_OnDefeated;
            BossLevelEndEffectComponent.OnShouldPlayEffects -= BossLevelEndEffectComponent_OnShouldPlayEffects;
        }


        private void PlayEffect()
        {
            StopCheckingConditions();

            OnShouldPlayEffect?.Invoke(levelTarget);
        }
        
        #endregion



        #region Events handlers

        private void LevelTarget_OnDefeated(LevelTarget anotherLevelTarget) =>
            PlayEffect();


        private void BossLevelEndEffectComponent_OnShouldPlayEffects() =>
            PlayEffect();

        #endregion
    }
}
