
using UnityEngine;


namespace Drawmasters.Levels
{
    public class PathShotWeapon : Weapon
    {
        #region Class lifecycle

        public PathShotWeapon(WeaponType type, Transform _projectilesSpawnRoot) 
            : base(type, _projectilesSpawnRoot) { }

        #endregion



        #region Methods

        public override void Shot(Vector2[] trajectory)
        {
            Content.Management.CreateProjectile(projectileType, Type, shooterColorType, trajectory, projectilesSpawnRoot);

            Vector2 directionVector = Vector2.right;
            TriggerShotCallback(directionVector);
        }

        #endregion
    }
}

