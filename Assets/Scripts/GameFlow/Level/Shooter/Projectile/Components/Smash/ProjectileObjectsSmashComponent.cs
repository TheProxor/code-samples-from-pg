namespace Drawmasters.Levels
{
    public class ProjectileObjectsSmashComponent : ProjectileSmashHandlerComponent
    {
        #region Class lifecycle

        public ProjectileObjectsSmashComponent(CollisionNotifier _projectileCollisionNotifier) : base(_projectileCollisionNotifier)
        {
        }

        #endregion



        #region Methods

        protected override bool AllowSmash(CollidableObject collidableObject) =>
            collidableObject.PhysicalLevelObject != null &&
            collidableObject.PhysicalLevelObject.PhysicalData.type != PhysicalLevelObjectType.Dynamite &&
            collidableObject.PhysicalLevelObject.PhysicalData.type != PhysicalLevelObjectType.Bonus;

        #endregion
    }
}

