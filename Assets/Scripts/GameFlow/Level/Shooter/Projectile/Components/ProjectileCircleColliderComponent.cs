using UnityEngine;


namespace Drawmasters.Levels
{
    public class ProjectileCircleColliderComponent : ProjectileSettingsComponent
    {
        #region Fields

        [SerializeField] private CircleCollider2D circleCollider2D = default;

        #endregion



        #region Lifecycle

        public ProjectileCircleColliderComponent(CircleCollider2D _circleCollider2D)
        {
            circleCollider2D = _circleCollider2D;
        }

        #endregion



        #region Methods

        protected override void ApplySettings(WeaponSettings settings)
        {
            if (circleCollider2D.isTrigger)
            {
                if (settings is IProjectileTriggerColliderRadiusSettings colliderRadiusSettings)
                {
                    circleCollider2D.radius = colliderRadiusSettings.TriggerColliderRadius;
                }
                else
                {
                    LogError(settings.GetType().Name, nameof(IProjectileTriggerColliderRadiusSettings));
                }
            }
            else
            {
                if (settings is IProjectilePhysicsColliderRadiusSettings colliderRadiusSettings)
                {
                    circleCollider2D.radius = colliderRadiusSettings.PhysicsColliderRadius;
                }
                else
                {
                    LogError(settings.GetType().Name, nameof(IProjectilePhysicsColliderRadiusSettings));
                }
            }
        }

        #endregion
    }
}
