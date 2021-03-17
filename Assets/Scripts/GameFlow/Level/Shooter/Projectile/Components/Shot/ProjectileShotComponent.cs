using UnityEngine;
using System;


namespace Drawmasters.Levels
{
    public abstract class ProjectileShotComponent : ProjectileSettingsComponent
    {
        #region Fields

        public static event Action<Projectile> OnShotProduced;

        #endregion



        #region Methods

        public override void Initialize(Projectile mainProjectileValue, WeaponType mode)
        {
            base.Initialize(mainProjectileValue, mode);

            mainProjectile.OnShouldShot += MainProjectile_OnShouldTrajectoryShot;
        }


        public override void Deinitialize()
        {
            mainProjectile.OnShouldShot -= MainProjectile_OnShouldTrajectoryShot;

            base.Deinitialize();
        }


        protected abstract void OnTrajectoryShot(Vector2[] trajectory);

        #endregion



        #region Events handlers

        private void MainProjectile_OnShouldTrajectoryShot(Vector2[] trajectory)
        {
            OnTrajectoryShot(trajectory);
            OnShotProduced?.Invoke(mainProjectile);
        }

        #endregion
    }
}
