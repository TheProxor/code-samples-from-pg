using UnityEngine;


namespace Drawmasters.Levels
{
    public class ProjectileCollisionVelocitySaveComponent : ProjectileComponent
    {
        #region Fields

        [SerializeField] private CollisionNotifier collisionNotifier = default;
        [SerializeField] private Rigidbody2D targetRigidbody = default;

        private PreviousFrameRigidbody2D previousFrameRigidbody;

        #endregion



        #region Lifecycle

        public ProjectileCollisionVelocitySaveComponent(CollisionNotifier _collisionNotifier,
                                                        Rigidbody2D _targetRigidbody)
        {
            collisionNotifier = _collisionNotifier;
            targetRigidbody = _targetRigidbody;
        }

        #endregion



        #region Methods

        public override void Initialize(Projectile mainProjectileValue, WeaponType mode)
        {
            base.Initialize(mainProjectileValue, mode);

            previousFrameRigidbody = new PreviousFrameRigidbody2D(targetRigidbody);

            collisionNotifier.OnCustomCollisionEnter2D += CollisionNotifier_OnCustomCollisionEnter2D;
            ProjectileShotComponent.OnShotProduced += ProjectileShotComponent_OnShotProduced;
        }


        public override void Deinitialize()
        {
            previousFrameRigidbody.Deinitialize();

            collisionNotifier.OnCustomCollisionEnter2D -= CollisionNotifier_OnCustomCollisionEnter2D;
            ProjectileShotComponent.OnShotProduced -= ProjectileShotComponent_OnShotProduced;

            base.Deinitialize();
        }

        #endregion



        #region Events handlers

        private void ProjectileShotComponent_OnShotProduced(Projectile projectile)
        {
            if (mainProjectile.Equals(projectile))
            {
                previousFrameRigidbody.Initialize();
            }
        }


        private void CollisionNotifier_OnCustomCollisionEnter2D(GameObject referenceGameObject, Collision2D collision2D)
        {
            mainProjectile.MainRigidbody2D.velocity = mainProjectile.MainRigidbody2D.velocity.normalized * previousFrameRigidbody.Velocity.magnitude;
        }

        #endregion
    }
}
