using System;


namespace Drawmasters.Levels
{
    public class ProjectileStayApplyComponent : ProjectileComponent
    {
        #region Fields

        public static event Action<Projectile> OnStopProjectile;

        #endregion



        #region Methods

        public override void Initialize(Projectile mainProjectileValue, WeaponType type)
        {
            base.Initialize(mainProjectileValue, type);

            mainProjectile.OnShouldStop += MainProjectile_OnShouldStop;
        }


        public override void Deinitialize()
        {
            mainProjectile.OnShouldStop -= MainProjectile_OnShouldStop;

            base.Deinitialize();
        }

        #endregion



        #region Events handlers

        private void MainProjectile_OnShouldStop()
        {
            OnStopProjectile?.Invoke(mainProjectile);
        }
    }

    #endregion
}
