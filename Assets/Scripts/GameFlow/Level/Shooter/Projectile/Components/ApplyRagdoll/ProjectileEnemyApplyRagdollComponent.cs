namespace Drawmasters.Levels
{
    public class ProjectileEnemyApplyRagdollComponent : ProjectileApplyRagdollComponent
    {
        #region Ctor

        public ProjectileEnemyApplyRagdollComponent(CollisionNotifier _collisionNotifier) 
            : base(_collisionNotifier) { }

        #endregion



        #region Abstract implementation

        protected override bool CanHitTarget(LevelTarget levelTarget, CollidableObjectType collidableObjectType)
        {
            bool isLevelTarget = collidableObjectType == CollidableObjectType.EnemyTrigger || collidableObjectType == CollidableObjectType.EnemyStand;

            return isLevelTarget &&
                   levelTarget.Type != LevelTargetType.Shooter;
        }

        #endregion
    }
}
