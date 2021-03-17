using System;


namespace Drawmasters.Levels
{
    public class BossLevelEndEffectComponent : PhysicalLevelObjectComponent
    {
        #region Fields

        public static event Action OnShouldPlayEffects;

        private BossLevelSettings bossLevelTargetSettings;

        #endregion



        #region Methods

        public override void Enable()
        {
            bossLevelTargetSettings = IngameData.Settings.bossLevelSettings;

            if (sourceLevelObject.CurrentData.shouldPlayEffectsOnPush)
            {
                MonoBehaviourLifecycle.OnUpdate += MonoBehaviourLifecycle_OnUpdate;
            }
        }


        public override void Disable()
        {
            MonoBehaviourLifecycle.OnUpdate -= MonoBehaviourLifecycle_OnUpdate;
        }


        private void PlayEffect()
        {
            OnShouldPlayEffects?.Invoke();
            MonoBehaviourLifecycle.OnUpdate -= MonoBehaviourLifecycle_OnUpdate;
        }

        #endregion



        #region Events handlers

        private void MonoBehaviourLifecycle_OnUpdate(float deltaTime)
        {
            float velocity = sourceLevelObject.Rigidbody2D.velocity.magnitude;

            if (velocity > bossLevelTargetSettings.objectVelocityForWinEffects)
            {
                PlayEffect();
            }
        }

        #endregion
    }
}
