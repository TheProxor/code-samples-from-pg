using UnityEngine;
using System.Collections.Generic;


namespace Drawmasters.Levels
{
    public class WeaponDestroy : PhysicalLevelObjectComponent
    {
        #region Fields

        [SerializeField] List<ProjectileType> projectileTypes = default;

        #endregion


        #region Ctor


        public WeaponDestroy()
        {

        }


        public WeaponDestroy(List<ProjectileType> _projectileTypes)
        {
            projectileTypes = _projectileTypes;
        }

        #endregion



        #region Abstract implementation

        public override void Enable()
        {
            if (collisionNotifier != null)
            {
                collisionNotifier.OnCustomTriggerEnter2D += CollisionNotifier_OnCustomTriggerEnter;
            }
        }


        public override void Disable()
        {
            if (collisionNotifier != null)
            {
                collisionNotifier.OnCustomTriggerEnter2D -= CollisionNotifier_OnCustomTriggerEnter;
            }
        }

        #endregion
        
        
        
        #region Protected methods

        protected virtual void PerformObjectDestroy()
        {
            sourceLevelObject.DestroyObject();
        }
        
        #endregion


        #region Events handlers

        private void CollisionNotifier_OnCustomTriggerEnter(GameObject referenceGameObject, Collider2D otherCollider)
        {
            CollidableObject collidable = otherCollider.GetComponent<CollidableObject>();

            if (collidable == null ||
                projectileTypes == null)
            {
                return;
            }

            Projectile projectile = collidable.Projectile;

            if (projectile != null)
            {
                bool isProjectileDestroy = projectileTypes.Contains(projectile.Type);

                if (isProjectileDestroy)
                {
                    PerformObjectDestroy();
                }
            }
        }

        #endregion
    }
}
