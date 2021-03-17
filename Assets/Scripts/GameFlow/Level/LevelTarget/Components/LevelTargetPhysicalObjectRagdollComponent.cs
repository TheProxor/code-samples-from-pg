using UnityEngine;


namespace Drawmasters.Levels
{
    public class LevelTargetPhysicalObjectRagdollComponent : LevelTargetComponent
    {
        #region Fields

        private CollisionNotifier collisionNotifier = default;

        #endregion



        #region Unity lifecycle

        public LevelTargetPhysicalObjectRagdollComponent(CollisionNotifier _collisionNotifier)
        {
            collisionNotifier = _collisionNotifier;
        }

        #endregion



        #region Methods

        public override void Enable()
        {
            collisionNotifier.OnCustomCollisionEnter2D += CollisionNotifier_OnCustomCollisionEnter2D;
        }


        public override void Disable()
        {
            collisionNotifier.OnCustomCollisionEnter2D -= CollisionNotifier_OnCustomCollisionEnter2D;
        }

        #endregion



        #region Events handlers

        private void CollisionNotifier_OnCustomCollisionEnter2D(GameObject go, Collision2D collision)
        {
            CollidableObject collidableObject = collision.gameObject.GetComponent<CollidableObject>();

            if (collidableObject == null)
            {
                return;
            }

            if (collidableObject.PhysicalLevelObject != null)
            {
                collisionNotifier.OnCustomCollisionEnter2D -= CollisionNotifier_OnCustomCollisionEnter2D;
                levelTarget.ApplyRagdoll();
            }
        }

        #endregion
    }
}
