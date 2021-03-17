using UnityEngine;
using Drawmasters.Levels;


namespace Drawmasters
{
    public class RagdollTriggerComponent : PhysicalLevelObjectComponent
    {
        #region Methods

        public override void Enable()
        {
            if (collisionNotifier != null)
            {
                collisionNotifier.OnCustomTriggerEnter2D += CollisionNotifier_OnCustomTriggerEnter2D;
            }
        }


        public override void Disable()
        {
            if (collisionNotifier != null)
            {
                collisionNotifier.OnCustomTriggerEnter2D -= CollisionNotifier_OnCustomTriggerEnter2D;
            }
        }


        void CollisionNotifier_OnCustomTriggerEnter2D(GameObject go, Collider2D collider)
        {
            CollidableObject collidableObject = collider.gameObject.GetComponent<CollidableObject>();

            if (collidableObject != null &&
                collidableObject.Type == CollidableObjectType.EnemyTrigger)
            {
                LevelTarget levelTarget = collidableObject.LevelTarget;
                
                if (levelTarget != null)
                {
                    levelTarget.ApplyRagdoll();
                }
            }
        }

        #endregion
    }
}
