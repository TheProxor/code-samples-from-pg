namespace Drawmasters.Levels
{
    public class ProjectileColoredEnemyApplyRagdollComponent : ProjectileEnemyApplyRagdollComponent
    {
        #region Ctor

        public ProjectileColoredEnemyApplyRagdollComponent(CollisionNotifier _collisionNotifier) 
            : base(_collisionNotifier)
        {
        }

        #endregion



        #region Abstract implementation

        protected override bool CanHitTarget(LevelTarget levelTarget, CollidableObjectType collidableObjectType)
        {
            bool result = base.CanHitTarget(levelTarget, collidableObjectType);

            result &= ColorTypesSolutions.CanHitEnemy(mainProjectile, levelTarget);

            return result;
        }

        #endregion
    }
}
