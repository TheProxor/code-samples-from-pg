using UnityEngine;
using System;


namespace Drawmasters.Levels
{
    public abstract class Weapon
    {
        #region Fields

        public static event Action<int, int> OnProjectilesCountChange;

        public event Action<Vector2> OnShot;


        protected ProjectileType projectileType;
        protected ShooterColorType shooterColorType;

        protected Transform weaponTransform;

        protected Transform projectilesSpawnRoot;

        protected int projectilesCount;

        #endregion



        #region Properties

        public WeaponType Type { get; private set; }


        public int ProjectilesCount
        {
            get => projectilesCount;
            set
            {
                int delta = value - projectilesCount;
                if (delta != 0)
                {
                    projectilesCount = value;
                    OnProjectilesCountChange?.Invoke(value, delta);
                }
            }
        }


        public virtual bool CanShoot => true;

        #endregion



        #region Class lifecycle

        protected Weapon(WeaponType type, 
                         Transform _projectilesSpawnRoot,
                         int _projectilesCount = int.MaxValue)
        {
            Type = type;
            projectilesSpawnRoot = _projectilesSpawnRoot;
            ProjectilesCount = _projectilesCount;
        }

        #endregion



        #region Methods

        public abstract void Shot(Vector2[] trajectory);

        public virtual void SubscribeToEvents() { }

        public virtual void UnsubscribeFromEvents() { }

        public virtual void Deinitialize()
        {
            OnShot = null;
        }


        public void SetupProjectileType(ProjectileType _projectileType) =>
            projectileType = _projectileType;


        public void SetupShooterColorType(ShooterColorType _shooterColorType) =>
            shooterColorType = _shooterColorType;



        public void SetupWeaponTransform(Transform _weaponTransform) =>
            weaponTransform = _weaponTransform;


        protected void TriggerShotCallback(Vector2 directionVector) =>
            OnShot?.Invoke(directionVector);


        protected static Vector2 TrajectoryDirection(Vector2[] trajectoryArray)
        {
            Vector2 direction = Vector2.up;

            if (trajectoryArray.Length == 2)
            {
                direction = trajectoryArray[1] - trajectoryArray[0];
                direction.Normalize();
            }
            else
            {
                CustomDebug.Log($"Incorrect direction definition");
            }

            return direction;
        }

        #endregion
    }
}
