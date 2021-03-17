using System;
using Drawmasters.ServiceUtil;
using Modules.General;


namespace Drawmasters.Levels
{
    public class LevelTargetRocketLaunchComponent : LevelTargetComponent
    {
        #region Fields

        public static event Action<RocketLaunchData.Data[]> OnRocketsLaunched;
        public static event Action OnAllRocketsDestroyed;

        private RocketLaunchData[] stagesData;

        private EnemyBoss enemyBoss;

        private Weapon weapon;

        private LevelProjectileController projectileController;

        #endregion



        #region Class lifecycle

        public LevelTargetRocketLaunchComponent()
        {
            projectileController = GameServices.Instance.LevelControllerService.Projectile;
        }

        #endregion



        #region Methods

        public override void Enable()
        {
            enemyBoss = levelTarget as EnemyBoss;

            if (enemyBoss == null)
            {
                CustomDebug.Log($"No impleted logic for simple level target. Can't detect stages and appear callback in {this}");
            }

            stagesData = enemyBoss.RocketLaunchData;

            Level.OnLevelStateChanged += Level_OnLevelStateChanged;

            weapon = Content.Management.CreateWeapon(WeaponType.BossLauncher, enemyBoss.transform, enemyBoss.transform);

        }


        public override void Disable()
        {
            projectileController.OnProjectileLeftGameZone -= ProjectileController_OnProjectileLeftGameZone;
            Level.OnLevelStateChanged -= Level_OnLevelStateChanged;

            TouchManager.Instance.IsEnabled = true;
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
        }

        #endregion



        #region Events handlers

        private void Level_OnLevelStateChanged(LevelState state)
        {
            if (state == LevelState.FinishDrawing)
            {
                bool isDataExists = LevelStageController.CurrentStageIndex < stagesData.Length;
                RocketLaunchData.Data[] currentStageData = isDataExists ? stagesData[LevelStageController.CurrentStageIndex].data : Array.Empty<RocketLaunchData.Data>();

                foreach (var data in currentStageData)
                {
                    weapon.SetupShooterColorType(data.colorType);
                    weapon.Shot(data.trajectory.ToVector2Array());
                }

                OnRocketsLaunched?.Invoke(currentStageData);

                if (currentStageData.Length == 0)
                {
                    OnAllRocketsDestroyed?.Invoke();
                }
                else
                {
                    projectileController.OnProjectileLeftGameZone += ProjectileController_OnProjectileLeftGameZone;
                }
            }

            if (state == LevelState.ReturnToInitial)
            {
                projectileController.OnProjectileLeftGameZone -= ProjectileController_OnProjectileLeftGameZone;
            }
        }

        private void ProjectileController_OnProjectileLeftGameZone()
        {
            if (!projectileController.IsAnyActiveProjectileExists(ProjectileType.BossRocket))
            {
                OnAllRocketsDestroyed?.Invoke();
            }
        }

        #endregion
    }
}
