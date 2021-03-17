using UnityEngine;

namespace Drawmasters.Levels
{
    public class ProjectilePhysicsMaterialComponent : ProjectileSettingsComponent
    {
        #region Abstract implementation

        protected override void ApplySettings(WeaponSettings settings)
        {
            if (settings is IProjectilePhysicsMaterialSettings physicsMaterialSettings)    
            {
                mainProjectile.MainRigidbody2D.sharedMaterial = new PhysicsMaterial2D
                {
                    friction = physicsMaterialSettings.Friction,
                    bounciness = physicsMaterialSettings.Bounciness
                };
            }
            else
            {
                LogError(settings.GetType().Name, nameof(IProjectilePhysicsMaterialSettings));
            }
        }

        #endregion
    }
}
