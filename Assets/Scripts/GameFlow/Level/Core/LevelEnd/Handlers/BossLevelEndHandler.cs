using Drawmasters.Levels.Data;
using Drawmasters.ServiceUtil;
using System;
using Modules.General;


namespace Drawmasters.Levels
{
    public class BossLevelEndHandler : ILevelEndHandler
    {
        #region Fields

        private readonly LevelContext levelContext;
        private readonly Level level;
        private readonly Action<LevelState> onStateChanged;
        private readonly LevelTargetController targetController;

        #endregion



        #region ILevelEndHandler

        public event Action<LevelResult> OnEnded;

        #endregion



        #region Ctor

        public BossLevelEndHandler(Level _level,
                                     Action<LevelState> _onStateChanged)
        {
            levelContext = GameServices.Instance.LevelEnvironment.Context;
            level = _level;
            onStateChanged = _onStateChanged;
            targetController = GameServices.Instance.LevelControllerService.Target;
        }

        #endregion



        #region IInitializable

        public void Initialize()
        {
            targetController.OnTargetHitted += TargetController_OnTargetHitted;
        }

        #endregion


        #region IDeinitializable

        public void Deinitialize()
        {
            targetController.OnTargetHitted -= TargetController_OnTargetHitted;
            
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
        }

        #endregion



        #region Events handlers

        private void TargetController_OnTargetHitted(LevelTargetType hittedTargetType)
        {
            if (level.CurrentState == LevelState.AllTargetsHitted ||
                level.CurrentState == LevelState.EndPlaying)
            {
                return;
            }

            if (hittedTargetType.IsFriendly())
            {
                Scheduler.Instance.UnscheduleAllMethodForTarget(this);

                onStateChanged?.Invoke(LevelState.FriendlyDeath);
            }
            else
            {
                if (hittedTargetType.IsEnemy() &&
                    targetController.IsAllEnemiesHitted())
                {
                    Scheduler.Instance.UnscheduleAllMethodForTarget(this);

                    onStateChanged?.Invoke(LevelState.AllTargetsHitted);

                    Scheduler.Instance.CallMethodWithDelay(this, () =>
                    {
                        targetController.MarkFriendlyTargetsImmortal();

                        float winDelay = IngameData.Settings.level.bossLevelEndDelay;

                        Scheduler.Instance.CallMethodWithDelay(this, () => OnEnded?.Invoke(LevelResult.Complete), winDelay);

                    }, IngameData.Settings.level.GetGameplayDelay(levelContext.Mode));
                }
            }
        }

        #endregion
    }
}
