using System.Collections.Generic;

namespace Drawmasters.Levels
{
    public class HitmastersPortalgunProjectile : Projectile
    {
        #region Properties

        public float MonolithSpikesCastRadius => 15.0f;

        public float CastRadius => 2.0f; // why property?

        #endregion



        #region Overrided

        public override ProjectileType Type => ProjectileType.HitmastersPortalgun;

        protected override List<ProjectileComponent> CoreComponents
        {
            get
            {
                List<ProjectileComponent> result = base.CoreComponents;

                List<ProjectileComponent> additionalComponents = new List<ProjectileComponent>()
                {
                    new ProjectilePortalComponent(),
                    new ProjectileShooterDestroyOnCollisionComponent(typesThatDestroyProjectile,
                                                                     physicalLevelObjectTypes,
                                                                     projectileCollisionNotifier),
                    new ProjectileConstShotComponent(),
                    new ProjectileRotation(),
                    new PortalProjectileTrail()
                };

                result.AddRange(additionalComponents);

                return result;
            }
        }

        #endregion
    }
}
