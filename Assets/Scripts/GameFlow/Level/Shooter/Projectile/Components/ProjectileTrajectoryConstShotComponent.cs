using System;
using System.Linq;
using DG.Tweening;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class ProjectileTrajectoryConstShotComponent : ProjectileShotComponent
    {
        #region Fields

        private SniperSettings sniperSettings;

        private Vector2[] trajectory;
        private object rotateGuid;

        #endregion



        #region Public methods

        public override void Initialize(Projectile mainProjectileValue, WeaponType mode)
        {
            base.Initialize(mainProjectileValue, mode);

            rotateGuid = Guid.NewGuid();

            ProjectileSmashApplyComponent.OnSmashProjectile += StopProjectile;
            ProjectileStayApplyComponent.OnStopProjectile += StopProjectile;
            
            mainProjectile.OnShouldDestroy += MainProjectile_OnShouldDestroy;
        }


        public override void Deinitialize()
        {
            ProjectileSmashApplyComponent.OnSmashProjectile -= StopProjectile;
            ProjectileStayApplyComponent.OnStopProjectile -= StopProjectile;
            
            mainProjectile.OnShouldDestroy -= MainProjectile_OnShouldDestroy;

            DOTween.Kill(rotateGuid);
            DOTween.Kill(this);

            base.Deinitialize();
        }
        
        #endregion



        #region Protected

        protected override void OnTrajectoryShot(Vector2[] _trajectory)
        {
            trajectory = _trajectory;

            if (trajectory == null || trajectory.Length == 0)
            {
                return;
            }

            if (trajectory.Length == 1) // TODO no end direction. Implement this case later
            {
                return;
            }

            mainProjectile.MainRigidbody2D.bodyType = RigidbodyType2D.Dynamic;
            mainProjectile.MainRigidbody2D.velocity = Vector2.zero;
            mainProjectile.MainRigidbody2D.angularVelocity = default;
            mainProjectile.MainRigidbody2D.position = trajectory.FirstObject();

            float totalTrajectoryDistance = default;

            for (int i = 1; i < trajectory.Length; i++)
            {
                int prevIndex = i - 1;
                totalTrajectoryDistance += Vector2.Distance(trajectory[prevIndex], trajectory[i]);
            }

            float duration = default;

            if (sniperSettings is IProjectileSpeedSettings speedSettings)
            {
                if (speedSettings.Speed > 0f)
                {
                    duration = totalTrajectoryDistance / speedSettings.Speed;
                }
            }

            mainProjectile.MainRigidbody2D
                .DOPath(trajectory, duration, PathType.Linear, PathMode.TopDown2D)
                .SetId(this)
                .SetEase(Ease.Linear)
                .OnWaypointChange(OnWaypointChange);
        }


        private void OnWaypointChange(int value)
        {
            bool isEndPoint = (value + 1) >= trajectory.Length;
            Vector2 direction = isEndPoint ? (trajectory.Last() - trajectory[trajectory.Length - 2]) : trajectory[value + 1] - trajectory[value];
            direction = direction.normalized;

            float angleInRad = Mathf.Atan2(direction.y, direction.x);
            
            mainProjectile.MainRigidbody2D.velocity = sniperSettings.Speed * direction;

            Vector3 endRotationAngles = 
                mainProjectile.transform.eulerAngles.SetZ(angleInRad * Mathf.Rad2Deg);
            
            mainProjectile.transform
                .DORotate(endRotationAngles, sniperSettings.smoothDuration, RotateMode.Fast)
                .SetId(rotateGuid);
        }


        protected override void ApplySettings(WeaponSettings settings)
        {
            if (settings is SniperSettings _sniperSettings)
            {
                sniperSettings = _sniperSettings;
            }
        }


        private void StopProjectile(Projectile projectile)
        {
            if (mainProjectile == projectile)
            {
                DOTween.Kill(this);
                DOTween.Kill(rotateGuid);
                mainProjectile.MainRigidbody2D.bodyType = RigidbodyType2D.Dynamic;
            }
        }

        #endregion



        #region Events handlers

        private void MainProjectile_OnShouldDestroy(Projectile projectile)
        {
            StopProjectile(projectile);
        }

        #endregion
    }
}
