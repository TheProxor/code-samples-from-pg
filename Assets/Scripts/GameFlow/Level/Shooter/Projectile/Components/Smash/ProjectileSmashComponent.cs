namespace Drawmasters.Levels
{
    public class ProjectileSmashComponent : ProjectileSmashHandlerComponent
    {
        #region Class lifecycle

        public ProjectileSmashComponent(CollisionNotifier _projectileCollisionNotifier) : base(_projectileCollisionNotifier)
        {
        }

        #endregion



        #region Methods

        protected override bool AllowSmash(CollidableObject collidableObject)
        {
            bool result = false;

            if (collidableObject.Type == CollidableObjectType.Projectile)
            {
                result = ColorTypesSolutions.ShouldSmashProjectiles(mainProjectile, collidableObject.Projectile);
            }

            return result;
        }

        #endregion
    }
}

