using Drawmasters.Levels;
using Modules.General;
using UnityEngine;


namespace Drawmasters
{
    public class ProjectileDestroyAfterShotComponent : ProjectileSettingsComponent
    {
        #region Fields

        private float delay;

        #endregion



        #region Methods

        public override void Initialize(Projectile mainProjectileValue, WeaponType mode)
        {
            base.Initialize(mainProjectileValue, mode);

            mainProjectile.OnShouldShot += MainProjectile_OnShouldShot;
        }


        public override void Deinitialize()
        {
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);

            mainProjectile.OnShouldShot -= MainProjectile_OnShouldShot;

            base.Deinitialize();
        }


        protected override void ApplySettings(WeaponSettings settings)
        {
            switch (settings)
            {
                case SniperSettings sniperProjectileSettings:
                    delay = sniperProjectileSettings.lifeTime;
                    break;

                default:
                    CustomDebug.Log($"Apply logic for settings {settings} not implemented in {this}!");
                    break;
            }
        }

        #endregion



        #region Events handlers

        private void MainProjectile_OnShouldShot(Vector2[] trajectory)
        {
            Scheduler.Instance.CallMethodWithDelay(this, mainProjectile.Destroy, delay);
        }

        #endregion
    }
}
