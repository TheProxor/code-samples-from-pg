using UnityEngine;


namespace Drawmasters.Levels
{
    public abstract class ProjectileCollisionComponent : ProjectileComponent
    {
        #region Fields

        private readonly CollisionNotifier projectileCollisionNotifier;

        #endregion



        #region Lifecycle

        public ProjectileCollisionComponent(CollisionNotifier _projectileCollisionNotifier)
        {
            projectileCollisionNotifier = _projectileCollisionNotifier;
        }

        #endregion



        #region Methods

        public override void Initialize(Projectile mainProjectileValue, WeaponType type)
        {
            base.Initialize(mainProjectileValue, type);

            projectileCollisionNotifier.OnCustomTriggerEnter2D += CollisionNotifier_OnCustomTriggerEnter2D;
            projectileCollisionNotifier.OnCustomCollisionEnter2D += ProjectileCollisionNotifier_OnCustomCollisionEnter2D;

            mainProjectile.MainRigidbody2D.simulated = true;
        }


        public override void Deinitialize()
        {
            StopCheckCollisions();

            base.Deinitialize();
        }


        protected void StopCheckCollisions()
        {
            projectileCollisionNotifier.OnCustomTriggerEnter2D -= CollisionNotifier_OnCustomTriggerEnter2D;
            projectileCollisionNotifier.OnCustomCollisionEnter2D -= ProjectileCollisionNotifier_OnCustomCollisionEnter2D;
        }


        protected abstract void OnCollisionEnter(CollidableObject collidableObject);
        protected abstract void OnTriggerEnter(CollidableObject collidableObject);

        #endregion



        #region Events handlers

        private void CollisionNotifier_OnCustomTriggerEnter2D(GameObject reference, Collider2D collision)
        {
            CollidableObject collidableObject = collision.GetComponent<CollidableObject>();

            if (collidableObject != null)
            {
                OnTriggerEnter(collidableObject);
            }

        }


        private void ProjectileCollisionNotifier_OnCustomCollisionEnter2D(GameObject reference, Collision2D collision)
        {
            CollidableObject collidableObject = collision.gameObject.GetComponent<CollidableObject>();

            if (collidableObject != null)
            {
                OnCollisionEnter(collidableObject);
            }
        }
    }

    #endregion
}
