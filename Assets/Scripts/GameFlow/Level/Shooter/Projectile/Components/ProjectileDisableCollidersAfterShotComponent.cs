using System.Collections.Generic;
using Modules.General;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class ProjectileDisableCollidersAfterShotComponent : ProjectileComponent
    {
        #region Fields

        [SerializeField] private Collider2D[] collidersToDisable = default;

        [SerializeField] private float delay = default;

        #endregion



        #region Lifecycle

        public ProjectileDisableCollidersAfterShotComponent(List<Collider2D> colliders2D, float _delay)
        {
            collidersToDisable = colliders2D.ToArray();
            delay = _delay;
        }

        #endregion



        #region Methods

        public override void Initialize(Projectile mainProjectileValue, WeaponType mode)
        {
            base.Initialize(mainProjectileValue, mode);

            foreach (var c in collidersToDisable)
            {
                c.enabled = false;
            }

            mainProjectile.OnShouldShot += MainProjectile_OnShouldShot;
        }


        public override void Deinitialize()
        {
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);

            mainProjectile.OnShouldShot -= MainProjectile_OnShouldShot;

            base.Deinitialize();
        }

        #endregion



        #region Events handlers

        private void MainProjectile_OnShouldShot(Vector2[] path)
        {
            Scheduler.Instance.CallMethodWithDelay(this, () =>
            {
                foreach (var c in collidersToDisable)
                {
                    c.enabled = true;
                }
            }, delay);
        }

        #endregion
    }
}
