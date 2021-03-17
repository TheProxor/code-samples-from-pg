namespace Drawmasters.Levels
{
    public class ProjectileRigidbodySettingsComponent : ProjectileSettingsComponent
    {
        #region Methods

        protected override void ApplySettings(WeaponSettings settings)
        {
            if (settings is IProjectilePhysicsSettings physicsSettings)
            {
                mainProjectile.MainRigidbody2D.mass = physicsSettings.Mass;
                mainProjectile.MainRigidbody2D.gravityScale = physicsSettings.GravityScale;
            }
            else
            {
                LogError(settings.GetType().Name, nameof(IProjectilePhysicsSettings));
            }
        }

        #endregion
    }
}
