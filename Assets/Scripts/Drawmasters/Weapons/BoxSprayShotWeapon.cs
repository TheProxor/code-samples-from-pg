using UnityEngine;


namespace Drawmasters.Levels
{
    public partial class BoxSprayShotWeapon : Weapon
    {
        #region Fields

        private float spawnWidth;

        private readonly int projectilesPerShot;

        #endregion



        #region Class lifecycle

        public BoxSprayShotWeapon(WeaponType type, int _projectileCount, Transform root)             
            : base(type, root, _projectileCount)
        {
            WeaponSettings settings = IngameData.Settings.modesInfo.GetSettings(type);

            if (settings is HitmastersShotgunSettings shotgunSettings)
            {
                spawnWidth = shotgunSettings.spawnWidth;

                projectilesPerShot = shotgunSettings.projectilesCountPerShot;

                #if UNITY_EDITOR
                    shotgunModeSettings = shotgunSettings;
                #endif
            }
        }

        #endregion



        #region Overrided

        public override bool CanShoot => ProjectilesCount > 0;

        public override void SubscribeToEvents()
        {
            base.SubscribeToEvents();

            #if UNITY_EDITOR
                DebugSubscribeToEvents();
            #endif
        }


        public override void UnsubscribeFromEvents()
        {
            base.UnsubscribeFromEvents();

            #if UNITY_EDITOR
                DebugUnsubscribeFromEvents();
            #endif
        }

        public override void Shot(Vector2[] trajectory)
        {
            Vector2 direction = TrajectoryDirection(trajectory);

            Vector3 widthVector = Quaternion.Euler(0.0f, 0.0f, 90.0f) * direction * spawnWidth;
            Vector3 projectileOffsetVector = widthVector / (1 + projectilesPerShot);

            for (int i = 0; i < projectilesPerShot; i++)
            {
                Vector3 spawnPoint = projectilesSpawnRoot.position - (widthVector / 2) + projectileOffsetVector * (i + 1);  

                Content.Management.CreateProjectile(projectileType, 
                                                    Type, 
                                                    ShooterColorType.Default, 
                                                    trajectory, 
                                                    projectilesSpawnRoot,
                                                    spawnPoint);
            }

            ProjectilesCount--;

            TriggerShotCallback(direction);            
        }

        #endregion
    }
}