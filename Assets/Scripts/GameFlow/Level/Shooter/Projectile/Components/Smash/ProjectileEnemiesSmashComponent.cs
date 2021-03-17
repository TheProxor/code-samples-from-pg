using System;
using Drawmasters.ServiceUtil;

namespace Drawmasters.Levels
{
    public class ProjectileEnemiesSmashComponent : ProjectileSmashHandlerComponent
    {
        #region Class lifecycle

        public ProjectileEnemiesSmashComponent(CollisionNotifier _projectileCollisionNotifier) : base(_projectileCollisionNotifier)
        {
        }

        #endregion



        #region Fields

        public static event Action<LevelTarget> OnSmash;

        #endregion



        #region Methods

        protected override bool AllowSmash(CollidableObject collidableObject)
        {
            bool result = false;

            if (collidableObject.LevelTarget != null)
            {
                result = ColorTypesSolutions.ShouldSmashProjectile(mainProjectile, collidableObject.LevelTarget);
            } 

            return result; 
        }

        protected override void Smash(CollidableObject collidableObject)
        {
            if (collidableObject.LevelTarget != null)
            {
                OnSmash?.Invoke(collidableObject.LevelTarget);
            }
        }
        #endregion
    }
}

