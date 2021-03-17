using UnityEngine;


namespace Drawmasters.Levels
{
    public abstract class PhysicalLevelObjectComponent : LevelObjectComponent
    {
        #region Fields

        protected CollisionNotifier collisionNotifier;
        protected Rigidbody2D rigidbody2D;
        protected PhysicalLevelObject sourceLevelObject;

        #endregion



        #region Methods

        public virtual void Initialize(CollisionNotifier notifier, 
                                       Rigidbody2D rigidbody,
                                       PhysicalLevelObject sourceObject) 
        {
            collisionNotifier = notifier;
            rigidbody2D = rigidbody;
            sourceLevelObject = sourceObject;
        }

        #endregion
    }
}
