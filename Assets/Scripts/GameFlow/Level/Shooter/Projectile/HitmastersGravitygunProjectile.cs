using System.Collections.Generic;


namespace Drawmasters.Levels
{
    public class HitmastersGravitygunProjectile : Projectile
    {
        #region Abstract implementation

        public override ProjectileType Type => ProjectileType.HitmastersGravitygunBullet;

        #endregion



        #region Properties

        protected override List<ProjectileComponent> CoreComponents
        {
            get
            {
                var result = base.CoreComponents;

                result.Add(new ProjectilePullThrowComponent());
                result.Add(new ProjectileRotation());
                //result.Add(new ProjectileConstShotComponent());

                return result;
            }
        }

        #endregion
    }
}
