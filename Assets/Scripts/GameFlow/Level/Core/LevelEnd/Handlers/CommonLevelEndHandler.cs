using Drawmasters.Levels.Data;
using Drawmasters.ServiceUtil;
using Modules.Sound;
using System;
using Modules.General;


namespace Drawmasters.Levels
{
    public class CommonLevelEndHandler : ILevelEndHandler
    {
        #region Fields

        protected readonly Level level;
        protected readonly Action<LevelState> onStateChanged;

        private readonly LevelContext levelContext;
        private readonly LevelTargetController targetController;
        
        #endregion



        #region ILevelEndHandler

        public event Action<LevelResult> OnEnded;

        #endregion



        #region Ctor

        public CommonLevelEndHandler(Level _level,
                                     Action<LevelState> _onStateChanged)
        {
            levelContext = GameServices.Instance.LevelEnvironment.Context;
            level = _level;
            onStateChanged = _onStateChanged;
            targetController = GameServices.Instance.LevelControllerService.Target;
        }

        #endregion



        #region IInitializable

        public virtual void Initialize()
        {
            targetController.OnTargetHitted += TargetController_OnTargetHitted;
        }

        #endregion


        #region IDeinitializable

        public virtual void Deinitialize()
        {
            targetController.OnTargetHitted -= TargetController_OnTargetHitted;
            
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
        }

        #endregion



        #region Protected methods

        protected void InvokeLevelEndEvent(LevelResult resultType)
            => OnEnded?.Invoke(resultType);

        #endregion



        #region Events handlers

        private void TargetController_OnTargetHitted(LevelTargetType hittedTargetType)
        {
            if (level.CurrentState == LevelState.AllTargetsHitted ||
                level.CurrentState == LevelState.FriendlyDeath ||
                level.CurrentState == LevelState.EndPlaying)
            {
                return;
            }

            bool isFriendlyDeath = hittedTargetType.IsFriendly();

            if (isFriendlyDeath)
            {
                Scheduler.Instance.UnscheduleAllMethodForTarget(this);

                onStateChanged?.Invoke(LevelState.FriendlyDeath);

                SoundManager.Instance.PlaySound(AudioKeys.Music.LOSE_01);

                InvokeLevelEndEvent(LevelResult.Lose);
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

                        float winDelay = IngameData.Settings.level.allTargetHittedEndDelay;

                        Scheduler.Instance.CallMethodWithDelay(this, () => InvokeLevelEndEvent(LevelResult.Complete), winDelay);
                        SoundManager.Instance.PlayOneShot(AudioKeys.Music.WIN_01);

                    }, IngameData.Settings.level.GetGameplayDelay(levelContext.Mode));
                }
            }
        }

        #endregion
    }
}
