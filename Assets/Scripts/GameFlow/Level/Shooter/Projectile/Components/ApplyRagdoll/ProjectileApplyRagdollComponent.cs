using UnityEngine;


namespace Drawmasters.Levels
{
    public abstract class ProjectileApplyRagdollComponent : ProjectileComponent
    {
        #region Fields

        [SerializeField] private CollisionNotifier projectileCollisionNotifier = default;

        #endregion



        #region Properties

        protected abstract bool CanHitTarget(LevelTarget levelTarget, CollidableObjectType collidableObjectType);

        #endregion



        #region Lifecycle

        public ProjectileApplyRagdollComponent(CollisionNotifier _collisionNotifier)
        {
            projectileCollisionNotifier = _collisionNotifier;
        }

        #endregion



        #region Methods

        public override void Initialize(Projectile mainProjectileValue, WeaponType mode)
        {
            base.Initialize(mainProjectileValue, mode);

            projectileCollisionNotifier.OnCustomTriggerEnter2D += ProjectileCollisionNotifier_OnCustomTriggerEnter2D;
        }


        public override void Deinitialize()
        {
            base.Deinitialize();

            projectileCollisionNotifier.OnCustomTriggerEnter2D -= ProjectileCollisionNotifier_OnCustomTriggerEnter2D;
        }

        #endregion



        #region Events handlers

        private void ProjectileCollisionNotifier_OnCustomTriggerEnter2D(GameObject reference, Collider2D collision)
        {
            CollidableObject collidableObject = collision.transform.GetComponent<CollidableObject>();

            if (collidableObject == null)
            {
                return;
            }

            LevelTarget levelTarget = collidableObject.LevelTarget;

            if (levelTarget != null && CanHitTarget(levelTarget, collidableObject.Type))
            {
                levelTarget.ApplyRagdoll();
            }
        }

        #endregion
    }
}
