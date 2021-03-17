using UnityEngine;


namespace Drawmasters.Levels
{
    public class ProjectileRotation : ProjectileComponent
    {
        #region Fields

        private Transform rotatable = default;

        Vector2 previousVelocity;

        #endregion



        #region Methods

        public override void Initialize(Projectile mainProjectileValue, WeaponType mode)
        {
            base.Initialize(mainProjectileValue, mode);

            MonoBehaviourLifecycle.OnUpdate += MonoBehaviourLifecycle_OnUpdate;

            previousVelocity = mainProjectile.MainRigidbody2D.velocity;
            rotatable = mainProjectileValue.ProjectileSpriteRoot;
        }


        public override void Deinitialize()
        {
            MonoBehaviourLifecycle.OnUpdate -= MonoBehaviourLifecycle_OnUpdate;

            base.Deinitialize();
        }

        #endregion


        #region Events handlers

        void MonoBehaviourLifecycle_OnUpdate(float deltaTime)
        {
            if (rotatable != null)
            {
                Vector2 velocity = previousVelocity;
                float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;

                rotatable.rotation = Quaternion.Euler(0f, 0f, angle);
            }

            previousVelocity = mainProjectile.MainRigidbody2D.velocity;
        }

        #endregion
    }
}