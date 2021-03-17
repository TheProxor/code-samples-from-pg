using Drawmasters.Effects;
using Drawmasters.Levels.Data;
using Drawmasters.ServiceUtil;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class ShooterFxsComponent : ShooterComponent
    {
        #region Fields

        private string idleFxKey;
        private EffectHandler effectHandler;

        private LevelProjectileController projectileController;

        #endregion



        #region Methods

        public override void StartGame()
        {
            projectileController = GameServices.Instance.LevelControllerService.Projectile;
            idleFxKey = IngameData.Settings.shooterSkinsSettings.FindIdleFxsKey(shooter.ColorType);

            LevelContext context = GameServices.Instance.LevelEnvironment.Context;
            bool isSceneMode = context.SceneMode.IsSceneMode();
            bool isLiveOps = context.Mode.IsHitmastersLiveOps();

            if (isSceneMode || isLiveOps)
            {
                return;
            }

            ShootersInputLevelController.OnDraw += ShootersInputLevelController_OnStartDraw;
            Level.OnLevelStateChanged += Level_OnLevelStateChanged;
            GameServices.Instance.PetsService.InvokeController.OnInvokePetForLevel += PetsInvokeController_OnInvokePetForLevel;

            StartMonitorProjectiles();
        }


        public override void Deinitialize()
        {
            StopFx();

            ShootersInputLevelController.OnDraw -= ShootersInputLevelController_OnStartDraw;
            Level.OnLevelStateChanged -= Level_OnLevelStateChanged;
            GameServices.Instance.PetsService.InvokeController.OnInvokePetForLevel -= PetsInvokeController_OnInvokePetForLevel;

            StopMonitorProjectiles();
        }


        private void StartMonitorProjectiles()
        {
            projectileController.OnProjectileLeftGameZone += AttemptToStopFx;
            ProjectileSmashApplyComponent.OnSmashProjectile += OnProjectileStopped;
            ProjectileStayApplyComponent.OnStopProjectile += OnProjectileStopped;
        }


        private void StopMonitorProjectiles()
        {
            projectileController.OnProjectileLeftGameZone -= AttemptToStopFx;
            ProjectileSmashApplyComponent.OnSmashProjectile -= OnProjectileStopped;
            ProjectileStayApplyComponent.OnStopProjectile -= OnProjectileStopped;
        }


        private void PlayFx()
        {
            effectHandler = EffectManager.Instance.CreateSystem(idleFxKey,
                                                                true,
                                                                shooter.transform.position,
                                                                Quaternion.identity);
            if (effectHandler != null)
            {
                effectHandler.Play(withClear: false);
            }
        }


        private void StopFx()
        {
            EffectManager.Instance.ReturnHandlerToPool(effectHandler);

            effectHandler = null;
        }

        #endregion



        #region Events handlers

        private void ShootersInputLevelController_OnStartDraw(Shooter shooter, Vector2 position) =>
            StopFx();
        

        private void Level_OnLevelStateChanged(LevelState state)
        {
            switch (state)
            {
                case LevelState.Initialized:
                    StopFx();
                    PlayFx();
                    break;

                case LevelState.FriendlyDeath:
                case LevelState.AllTargetsHitted:
                    StopMonitorProjectiles();
                    StopFx();
                    break;

                case LevelState.ReturnToInitial:
                    StopMonitorProjectiles();
                    StartMonitorProjectiles();
                    break;

                default:
                    break;
            }
        }


        private void PetsInvokeController_OnInvokePetForLevel(PetSkinType petSkinType)
        {
            StopMonitorProjectiles();
            StopFx();
        }


        private void OnProjectileStopped(Projectile projectile) =>
            AttemptToStopFx();


        private void AttemptToStopFx()
        {
            if (projectileController.IsAllProjectilesStopped)
            {
                StopMonitorProjectiles();

                StopFx();
                PlayFx();
            }
        }

        #endregion
    }
}
