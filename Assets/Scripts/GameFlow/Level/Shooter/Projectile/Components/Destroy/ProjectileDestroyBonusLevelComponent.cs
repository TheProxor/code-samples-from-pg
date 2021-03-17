using Drawmasters.ServiceUtil;
using Drawmasters.Effects;
using UnityEngine;
using Modules.General;
using DG.Tweening;


namespace Drawmasters.Levels
{
    public class ProjectileDestroyBonusLevelComponent : ProjectileComponent
    {
        #region Fields

        private BonusLevelSettings levelSettings;

        private BonusLevelController bonusLevelController;

        #endregion



        #region Methods

        public override void Initialize(Projectile mainProjectileValue, WeaponType type)
        {
            base.Initialize(mainProjectileValue, type);

            levelSettings = IngameData.Settings.bonusLevelSettings;

            bonusLevelController = GameServices.Instance.LevelControllerService.BonusLevelController;

            if (bonusLevelController != null)
            {
                bonusLevelController.OnStartSpawn += BonusLevelController_OnStartSpawn;
                bonusLevelController.OnStageBegun += BonusLevelController_OnStageBegun;
            }
        }


        public override void Deinitialize()
        {
            DOTween.Kill(this, true);

            Scheduler.Instance.UnscheduleAllMethodForTarget(this);

            if (bonusLevelController != null)
            {
                bonusLevelController.OnStartSpawn -= BonusLevelController_OnStartSpawn;
                bonusLevelController.OnStageBegun -= BonusLevelController_OnStageBegun;
            }

            SetInitalScale();

            base.Deinitialize();
        }


        private void PlayProjectileDestroyEffect()
        {
            VectorAnimation projectileDestroyAnimation = levelSettings.projectileDestroyAnimation; 

            Scheduler.Instance.CallMethodWithDelay(
                         this,
                        () => EffectManager.Instance.CreateSystem(EffectKeys.FxGUIBonusLevelWeaponDisappear, true, mainProjectile.transform.position),
                        projectileDestroyAnimation.delay);

            projectileDestroyAnimation.Play(value => mainProjectile.transform.localScale = value,
                       this);
        }


        private void SetInitalScale() =>
            mainProjectile.transform.localScale = Vector3.one;
        
        #endregion



        #region Events handlers

        private void BonusLevelController_OnStartSpawn(int stage) => 
            PlayProjectileDestroyEffect();

        private void BonusLevelController_OnStageBegun(int stage) =>
            SetInitalScale();

        #endregion
    }
}
