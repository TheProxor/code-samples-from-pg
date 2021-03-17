using System.Collections.Generic;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class ImpulsePushComponent : PhysicalLevelObjectComponent
    {
        #region Fields

        private static readonly HashSet<PhysicalLevelObjectType> PetsPushTypes =
             new HashSet<PhysicalLevelObjectType> { PhysicalLevelObjectType.Metal };
        #endregion



        #region Overrided methods

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


        protected void HandleCollision(GameObject anotherGameObject)
        {
            CollidableObject collidable = anotherGameObject.GetComponent<CollidableObject>();
            if (collidable == null)
            {
                return;
            }

            if (collidable.Type == CollidableObjectType.Pet)
            {
                HandlePet(collidable);
            }
        }

        #endregion



        #region Methods

        private void HandlePet(CollidableObject collidableObject)
        {
            if (PetsPushTypes.Contains(sourceLevelObject.PhysicalData.type))
            {
                Vector2 impulsDirection = sourceLevelObject.transform.position - collidableObject.transform.position;
                float impulsMagnitude = IngameData.Settings.physicalObject.petsImpulsMagnitude;
                Vector2 impulsToAdd = impulsDirection.normalized * impulsMagnitude;
                sourceLevelObject.Rigidbody2D.AddForceAtPosition(impulsToAdd, collidableObject.transform.position);
            }
        }

        #endregion



        #region Events handlers

        private void CollisionNotifier_OnCustomTriggerEnter2D(GameObject go, Collider2D otherCollider) =>
            HandleCollision(otherCollider.gameObject);
        
        #endregion
    }
}

