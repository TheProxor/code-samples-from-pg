using UnityEngine;
using System.Collections.Generic;


namespace Drawmasters.Levels
{
    public class FixedJointObjectComponent : PhysicalLevelObjectComponent
    {
        #region Fields

        private FixedJoint2D fixedJoint2D;

        #endregion



        #region Methods

        public override void Initialize(CollisionNotifier notifier, Rigidbody2D rigidbody, PhysicalLevelObject sourceObject)
        {
            base.Initialize(notifier, rigidbody, sourceObject);
            
            BreakJoint();
            RemoveJoint();
            
            sourceLevelObject.OnLinksSet += SourceLevelObject_OnLinksSet;
            collisionNotifier.OnCustomTriggerEnter2D += CollisionNotifier_OnCustomTriggerEnter2D;
            collisionNotifier.OnCustomCollisionEnter2D += CollisionNotifier_OnCustomCollisionEnter2D;
        }

        public override void Enable(){ }


        public override void Disable()
        {
            sourceLevelObject.OnLinksSet -= SourceLevelObject_OnLinksSet;
            collisionNotifier.OnCustomTriggerEnter2D -= CollisionNotifier_OnCustomTriggerEnter2D;
            collisionNotifier.OnCustomCollisionEnter2D -= CollisionNotifier_OnCustomCollisionEnter2D;

            BreakJoint();
            RemoveJoint();
        }


        private void BreakJoint()
        {
            if (!fixedJoint2D.IsNull())
            {
                fixedJoint2D.enabled = false;
                fixedJoint2D.connectedBody = null;
            }
        }

        private void RemoveJoint()
        {
            if (!fixedJoint2D.IsNull())
            {
                Object.Destroy(fixedJoint2D);
                fixedJoint2D = null;
            }
        }

        #endregion



        #region Events handlers

        private void SourceLevelObject_OnLinksSet(List<LevelObject> linkedObjects)
        {
            if (sourceLevelObject.IsLinkedObjectsPart)
            {
                return;
            }

            LevelObject connectedObject = linkedObjects.First();

            if (connectedObject != null)
            {
                Rigidbody2D connectedBody = connectedObject.Rigidbody2D;

                // otherwise it makes object as static
                if (connectedBody != null)
                {
                    BreakJoint();
                    RemoveJoint();
                    
                    fixedJoint2D = sourceLevelObject.gameObject.AddComponent<FixedJoint2D>();
                    fixedJoint2D.connectedBody = connectedBody;
                    fixedJoint2D.enabled = true;
                }
            }
        }


        private void CollisionNotifier_OnCustomTriggerEnter2D(GameObject referenceObject, Collider2D collision)
        {
            CollidableObject collidableObject = collision.GetComponent<CollidableObject>();

            if (collidableObject == null)
            {
                return;
            }

            if (sourceLevelObject.PhysicalData.shapeType != PhysicalLevelObjectShapeType.Spikes &&
                sourceLevelObject.PhysicalData.type != PhysicalLevelObjectType.Metal &&
                collidableObject.Projectile != null)
            {
                BreakJoint(); 
                RemoveJoint();
            }
        }


        private void CollisionNotifier_OnCustomCollisionEnter2D(GameObject referenceObject, Collision2D collision)
        {
            CollidableObject collidableObject = collision.gameObject.GetComponent<CollidableObject>();

            if (collidableObject == null)
            {
                return;
            }

            if (sourceLevelObject.PhysicalData.shapeType != PhysicalLevelObjectShapeType.Spikes &&
                sourceLevelObject.PhysicalData.type != PhysicalLevelObjectType.Metal &&
                collidableObject.Projectile != null)
            {
                BreakJoint();
                RemoveJoint();
            }
        }

        #endregion
    }
}
