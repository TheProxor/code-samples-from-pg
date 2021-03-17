using Drawmasters.Levels;
using UnityEngine;


namespace Drawmasters.Utils
{
    public static class CollisionUtility
    {
        #region Methods

        public static Rigidbody2D FindLevelObjectRigidbody(Collider2D collision)
        {
            Rigidbody2D result = default;

            CollidableObject collidableObject = collision.GetComponent<CollidableObject>();

            if (collidableObject == null)
            {
                return result;
            }

            if (collidableObject.Type == CollidableObjectType.EnemyTrigger)
            {
                result = FindLevelTargetRigidbody(collidableObject);
            }
            else
            {
                PhysicalLevelObject levelObject = collidableObject.PhysicalLevelObject;
                if (levelObject == null)
                {
                    return result;
                }

                result = levelObject.Rigidbody2D;
            }

            return result;
        }

        public static bool IsContainsLevelTargetRigidbody(CollidableObject collidableObject)
        {
            LevelTarget levelTarget = collidableObject.LevelTarget;
            LevelTargetLimbPart levelTargetLimbPart = collidableObject.GetComponent<LevelTargetLimbPart>();

            bool result = levelTarget != null && levelTargetLimbPart != null;
            return result;
        }


        public static Rigidbody2D FindLevelTargetRigidbody(CollidableObject collidableObject)
        {
            Rigidbody2D result = default;

            LevelTarget levelTarget = collidableObject.LevelTarget;
            LevelTargetLimbPart levelTargetLimbPart = collidableObject.GetComponent<LevelTargetLimbPart>();

            if (!IsContainsLevelTargetRigidbody(collidableObject))
            {
                CustomDebug.Log($"Collidable object with type = {collidableObject.Type} isn't LevelTarget or LevelTargetLimbPart");
                return result;
            }

            result = (levelTarget.Ragdoll2D.IsActive) ?
                     levelTarget.Ragdoll2D.GetRigidbody(levelTargetLimbPart.BoneName) : levelTarget.StandRigidbody;

            return result;
        }

        #endregion
    }
}
