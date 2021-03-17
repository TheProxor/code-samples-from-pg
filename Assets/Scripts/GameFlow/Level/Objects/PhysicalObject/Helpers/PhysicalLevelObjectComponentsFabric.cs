using System.Collections.Generic;


namespace Drawmasters.Levels
{
    public static class PhysicalLevelObjectComponentsFabric
    {
        #region Public methods

        public static List<PhysicalLevelObjectComponent> GetNecessaryComponents(PhysicalLevelObjectData data)
        {
            List<PhysicalLevelObjectComponent> result = new List<PhysicalLevelObjectComponent>();

            switch(data.type)
            {
                case PhysicalLevelObjectType.Wood:
                    result.Add(new WeaponDestroy(new List<ProjectileType> { ProjectileType.Arrow,
                                                                            ProjectileType.HitmastersSniperBullet,
                                                                            ProjectileType.HitmastersShotgunBullet
                                                                          }));
                    break;

                case PhysicalLevelObjectType.Dynamite:
                    result.Add(new WeaponDestroy(new List<ProjectileType> { ProjectileType.Arrow,
                                                                            ProjectileType.BossRocket,
                                                                            ProjectileType.HitmastersSniperBullet,
                                                                            ProjectileType.HitmastersShotgunBullet
                                                                          }));
                    break;

                case PhysicalLevelObjectType.Metal:
                    result.Add(new ImpulsePushComponent());
                    break;
            }

            return result;
        }

        #endregion
    }
}

