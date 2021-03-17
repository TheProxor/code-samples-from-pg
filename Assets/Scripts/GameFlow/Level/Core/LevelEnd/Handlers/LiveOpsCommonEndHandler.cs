using Drawmasters.ServiceUtil;
using Modules.General;
using Modules.Sound;
using System;
using System.Linq;

namespace Drawmasters.Levels
{
    public class LiveOpsCommonEndHandler : CommonLevelEndHandler
    {
        #region Fields

        private readonly LevelProjectileController projectileController;
        private readonly Shooter shooter;

        #endregion



        #region Ctor

        public LiveOpsCommonEndHandler(Level _level, Action<LevelState> _onStateChanged) 
            : base(_level, _onStateChanged)
        {
            projectileController = GameServices.Instance.LevelControllerService.Projectile;

            shooter = GameServices.Instance.LevelControllerService.Target.GetShooters().FirstOrDefault();
        }

        #endregion



        #region Overrided

        public override void Initialize()
        {
            base.Initialize();

            projectileController.OnProjectileLeftGameZone += ProjectileController_OnProjectileLeftGameZone;
        }


        public override void Deinitialize()
        {
            projectileController.OnProjectileLeftGameZone -= ProjectileController_OnProjectileLeftGameZone;
            
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);

            base.Deinitialize();
        }

        #endregion



        #region Events handlers

        private void ProjectileController_OnProjectileLeftGameZone()
        {
            if (level.CurrentState == LevelState.Playing)
            {
                bool isOutOfProjectiles = !projectileController.IsAnyActiveProjectileExists() &&                                          
                                          shooter.ProjectilesCount == 0;

                if (isOutOfProjectiles)
                {
                    onStateChanged?.Invoke(LevelState.OutOfAmmo);

                    float loseDelay = IngameData.Settings.level.loseEndDelay;

                    Scheduler.Instance.CallMethodWithDelay(this, () => InvokeLevelEndEvent(LevelResult.Lose), loseDelay);

                    SoundManager.Instance.PlaySound(AudioKeys.Music.LOSE_01);

                    if (GameServices.Instance.LevelEnvironment.Context.IsBossLevel)
                    {
                        SoundManager.Instance.PlaySound(AudioKeys.Ingame.BOSSLAUGHTER);
                    }
                }
            }
        }

        #endregion
    }
}

