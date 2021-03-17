using UnityEngine;


namespace Drawmasters.Levels
{
    public class SingleShotWeapon : Weapon
    {
        #region Class lifecycle

        public SingleShotWeapon(WeaponType type, 
                                Transform _projectilesSpawnRoot,
                                int projectilesCount) 
            : base(type, 
                   _projectilesSpawnRoot,
                   projectilesCount)
        { }

        #endregion



        #region Overrided

        public override bool CanShoot => ProjectilesCount > 0;


        public override void Shot(Vector2[] trajectory)
        {
            Content.Management.CreateProjectile(projectileType, Type, ShooterColorType.Default, trajectory, projectilesSpawnRoot);

            ProjectilesCount--;

            Vector2 directon = TrajectoryDirection(trajectory);

            TriggerShotCallback(directon);            
        }

        #endregion
    }
}
