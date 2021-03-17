using UnityEngine;


namespace Drawmasters.Levels
{
    public class ProjectileConstShotComponent : ProjectileShotComponent
    {
        #region Fields

        private float projectileSpeed;

        #endregion



        #region Methods

        protected override void OnTrajectoryShot(Vector2[] trajectory)
        {
            Vector2 directionVector = (trajectory.LastObject() - trajectory.FirstObject()).normalized;
            float angleInRad = Mathf.Atan2(directionVector.y, directionVector.x);
            Vector2 velocity = projectileSpeed * new Vector2(Mathf.Cos(angleInRad), Mathf.Sin(angleInRad));

            mainProjectile.MainRigidbody2D.velocity = velocity;
            mainProjectile.transform.eulerAngles = mainProjectile.transform.eulerAngles.SetZ(angleInRad * Mathf.Rad2Deg);

        }


        protected override void ApplySettings(WeaponSettings settings)
        {
            if (settings is IProjectileSpeedSettings speedSettings)
            {
                projectileSpeed = speedSettings.Speed;
            }
            else
            {
                LogError(settings.GetType().Name, nameof(IProjectileSpeedSettings));
            }
        }

        #endregion
    }
}
