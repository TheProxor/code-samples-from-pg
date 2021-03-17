using UnityEngine;
using DG.Tweening;
using Drawmasters.Effects;
using Modules.General;


namespace Drawmasters.Levels
{
    public class ProjectileLaserHitComponent : ProjectileSettingsComponent
    {
        #region Fields

        private readonly SpriteRenderer mainRenderer;
        private ColorAnimation colorAnimation;

        private float destroyDelay;

        #endregion



        #region Class lifecycle

        public ProjectileLaserHitComponent(SpriteRenderer _mainRenderer)
        {
            mainRenderer = _mainRenderer;
        }

        #endregion



        #region Methods

        public override void Initialize(Projectile mainProjectileValue, WeaponType type)
        {
            base.Initialize(mainProjectileValue, type);

            mainRenderer.color = Color.white;

            mainProjectile.OnShouldLaserDestroy += MainProjectile_OnShouldLaserDestroy;
        }


        public override void Deinitialize()
        {
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
            DOTween.Kill(this);
            mainProjectile.OnShouldLaserDestroy -= MainProjectile_OnShouldLaserDestroy;

            base.Deinitialize();
        }


        protected override void ApplySettings(WeaponSettings settings)
        {
            if (settings is IProjectileLaserDelayDestroy projectileLaserDelayDestroy)
            {
                destroyDelay = projectileLaserDelayDestroy.LaserDestroyDelay;
                colorAnimation = projectileLaserDelayDestroy.LaserDestroyColorAnimation;
            }
            else
            {
                CustomDebug.LogWarning($"Trying use incorrect component for settings without {nameof(IProjectileLaserDelayDestroy)} implementation");
            }
        }

        #endregion



        #region Events handlers

        private void MainProjectile_OnShouldLaserDestroy(bool isImmediately)
        {
            mainProjectile.OnShouldLaserDestroy -= MainProjectile_OnShouldLaserDestroy;

            if (isImmediately)
            {
                EffectManager.Instance.PlaySystemOnce(EffectKeys.FxProjectileLaserDestroyed, 
                    mainProjectile.transform.position, 
                    mainProjectile.transform.rotation);
                
                mainProjectile.Destroy();
            }
            else
            {
                colorAnimation.Play((value) => mainRenderer.color = value, this, () =>
                {
                    EffectManager.Instance.PlaySystemOnce(EffectKeys.FxProjectileLaserDestroyed, 
                        mainProjectile.transform.position, 
                        mainProjectile.transform.rotation);
                });

                Scheduler.Instance.CallMethodWithDelay(this, () => mainProjectile.Destroy(), destroyDelay);
            }
        }

        #endregion
    }
}