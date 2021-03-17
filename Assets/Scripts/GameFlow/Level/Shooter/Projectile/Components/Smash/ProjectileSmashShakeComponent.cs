namespace Drawmasters.Levels
{
    public class ProjectileSmashShakeComponent : ProjectileComponent
    {
        #region Class lifecycle

        public ProjectileSmashShakeComponent() { }

        #endregion



        #region Methods

        public override void Initialize(Projectile mainProjectileValue, WeaponType type)
        {
            base.Initialize(mainProjectileValue, type);

            mainProjectile.OnShouldSmash += MainProjectile_OnShouldSmash;
        }


        public override void Deinitialize()
        {
            mainProjectile.OnShouldSmash -= MainProjectile_OnShouldSmash;

            base.Deinitialize();
        }

        #endregion



        #region Events handlers

        private void MainProjectile_OnShouldSmash(CollidableObject collidableObject)
        {
            CameraShakeSettings.Shake shake = IngameData.Settings.projectileSmashShakeSettings.FindShake(collidableObject.Type);

            if (shake != null)
            {
                mainProjectile.OnShouldSmash -= MainProjectile_OnShouldSmash;

                IngameCamera.Instance.Shake(shake);
            }
        }

        #endregion
    }
}

