namespace Drawmasters.Levels
{
    public abstract class ProjectileComponent
    {
        #region Fields

        protected Projectile mainProjectile;

        protected WeaponType currentWeaponType;

        #endregion



        #region Methods

        public virtual void Initialize(Projectile mainProjectileValue, WeaponType type)
        {
            mainProjectile = mainProjectileValue;
            currentWeaponType = type;
        }


        public virtual void Deinitialize()
        {
        }

        #endregion
    }
}
